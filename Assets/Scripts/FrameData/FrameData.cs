using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class FrameData : MonoBehaviour
{
    public new Collider collider
    {
        get
        {
            return GetComponent<Collider>();
        }
    }
    [HideInInspector]
    public new Rigidbody rigidbody;
    [HideInInspector]
    public MovingObject mo;

    [System.Serializable]
    public class ApplyMovement
    {
        public bool additive = true;
        public MathStuff.SortVectors.ApplyType type;
        public Vector3 amount;

        public void Do(bool continuous, FrameData hit, FrameData hurt = null, MathStuff.SortVectors.VectorType isTorque = MathStuff.SortVectors.VectorType.Force)
        {
            float multiplier = !continuous ? 1.0f : Time.deltaTime;

            if (hurt == null)
            {
                hurt = hit;
            }
            else if(hurt is HurtBox)
            {
                HurtBox _hurt = hurt as HurtBox;

                if (isTorque == MathStuff.SortVectors.VectorType.Force)
                    multiplier = _hurt.force.GetAngle(multiplier);
                else
                    multiplier = _hurt.torque.GetAngle(multiplier);
            }

            Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, amount, hit.transform, hit.rigidbody.transform, hurt.transform, hurt.rigidbody.transform, isTorque) * multiplier;

            if (isTorque == MathStuff.SortVectors.VectorType.Force)
            {   
                if (additive)
                    hurt.rigidbody.AddForce(vector, ForceMode.VelocityChange);
                else
                    hurt.rigidbody.velocity = vector;
            }
            else
            {
                if (additive)
                    hurt.rigidbody.AddTorque(vector, ForceMode.VelocityChange);
                else
                    hurt.rigidbody.angularVelocity = vector;
            }
        }
    }
    [System.Serializable]
    public class ApplyStat
    {
        public bool additive = true;
        public float amount;

        public float Do(float refAmount)
        {
            if (additive)
                return refAmount + amount;
            else
                return amount;
        }
    }

    [System.Serializable]
    public class Spawn
    {
        public GameObject spawnPrefab;

        public MathStuff.SortVectors.ApplyType positionType;
        public Vector3 position;

        public MathStuff.SortVectors.ApplyType rotationType;
        public Vector3 rotation;

        public ApplyMovement force;
        public ApplyMovement torque;

        public void Do(bool continuous, FrameData hit, FrameData hurt = null)
        {
            Vector3 pos = MathStuff.SortVectors.GetCorrectVector(positionType, position, hit.transform, hit.rigidbody.transform, hurt.transform, hurt.rigidbody.transform, MathStuff.SortVectors.VectorType.Position);
            Vector3 rot = MathStuff.SortVectors.GetCorrectVector(rotationType, rotation, hit.transform, hit.rigidbody.transform, hurt.transform, hurt.rigidbody.transform, MathStuff.SortVectors.VectorType.Rotation);

            GameObject go = Instantiate(spawnPrefab, pos, Quaternion.Euler(rot)) as GameObject;
            HitBox _hit = go.GetComponent<HitBox>();
            _hit.exclude.Add(hit.mo);
            HurtBox _hurt = go.GetComponent<HurtBox>();

            force.Do(continuous, hit, _hurt);
            torque.Do(continuous, hit, _hurt, MathStuff.SortVectors.VectorType.Torque);
        }
    }

    public virtual void Awake()
    {
        rigidbody = collider.attachedRigidbody;
        mo = rigidbody.GetComponent<MovingObject>();
    }
}
