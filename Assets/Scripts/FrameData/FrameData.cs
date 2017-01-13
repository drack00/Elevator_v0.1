using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
        public SortVectors.ApplyType type;
        public SortVectors.Flatten flatten;
        public Vector3 amount;

        public void Do(bool continuous, FrameData hit, FrameData hurt = null, SortVectors.VectorType isTorque = SortVectors.VectorType.Force)
        {
            float multiplier = !continuous ? 1.0f : Time.deltaTime;

            if (hurt == null)
            {
                hurt = hit;
            }
            else if(hurt is HurtBox)
            {
                HurtBox _hurt = hurt as HurtBox;

                if (isTorque == SortVectors.VectorType.Force)
                    multiplier = _hurt.force.GetAngle(multiplier);
                else
                    multiplier = _hurt.torque.GetAngle(multiplier);
            }

            Vector3 vector = SortVectors.GetCorrectVector(type, flatten, amount,
                hit.transform, hit.rigidbody.transform, hurt.transform, hurt.rigidbody.transform, 
                isTorque) * multiplier;

            if (isTorque == SortVectors.VectorType.Force)
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
        public int maxSpawns;
        private List<GameObject> gos = new List<GameObject>();

        public GameObject spawnPrefab;

        public SortVectors.ApplyType positionType;
        public SortVectors.Flatten positionFlatten;
        public Vector3 position;

        public SortVectors.ApplyType rotationType;
        public SortVectors.Flatten rotationFlatten;
        public Vector3 rotation;

        public ApplyMovement force;
        public ApplyMovement torque;

        public void Do(bool continuous, FrameData hit, FrameData hurt = null)
        {
            if (gos.Count >= maxSpawns && maxSpawns >= 0)
                return;

            Vector3 pos = SortVectors.GetCorrectVector(positionType, positionFlatten, position,
                hit.transform, hit.rigidbody.transform, hurt.transform, hurt.rigidbody.transform, 
                SortVectors.VectorType.Position);
            Vector3 rot = SortVectors.GetCorrectVector(rotationType, rotationFlatten, rotation, 
                hit.transform, hit.rigidbody.transform, hurt.transform, hurt.rigidbody.transform, 
                SortVectors.VectorType.Rotation);

            GameObject go = Instantiate(spawnPrefab, pos, Quaternion.Euler(rot)) as GameObject;
            gos.Add(go);
            HitBox _hit = go.GetComponent<HitBox>();
            _hit.exclude.Add(hit.mo);
            HurtBox _hurt = go.GetComponent<HurtBox>();

            force.Do(continuous, hit, _hurt);
            torque.Do(continuous, hit, _hurt, SortVectors.VectorType.Torque);
        }
    }

    public virtual void Awake()
    {
        rigidbody = collider.attachedRigidbody;
        mo = rigidbody.GetComponent<MovingObject>();
    }
}
