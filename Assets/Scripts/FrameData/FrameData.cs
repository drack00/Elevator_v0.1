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

    [System.Serializable]
    public class ApplyMovement
    {
        public bool additive = true;
        public MathStuff.SortVectors.ApplyType type;
        public Vector3 amount;

        public void Do(bool continuous, FrameData hit, FrameData hurt = null, bool isTorque = false)
        {
            float multiplier = !continuous ? 1.0f : Time.deltaTime;

            if (hurt == null)
            {
                hurt = hit;
            }
            else if(hurt is HurtBox)
            {
                HurtBox _hurt = hurt as HurtBox;

                if (!isTorque)
                    multiplier = _hurt.force.GetAngle(multiplier);
                else
                    multiplier = _hurt.torque.GetAngle(multiplier);
            }

            Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, amount, hit.collider, hurt.collider, isTorque) * multiplier;

            if (!isTorque)
            {   
                if (additive)
                    hurt.collider.attachedRigidbody.AddForce(vector, ForceMode.VelocityChange);
                else
                    hurt.collider.attachedRigidbody.velocity = vector;
            }
            else
            {
                if (additive)
                    hurt.collider.attachedRigidbody.AddTorque(vector, ForceMode.VelocityChange);
                else
                    hurt.collider.attachedRigidbody.angularVelocity = vector;
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
            Vector3 pos = MathStuff.SortVectors.GetCorrectVector(positionType, position, hit.collider, hurt.collider);
            Vector3 rot = MathStuff.SortVectors.GetCorrectVector(rotationType, rotation, hit.collider, hurt.collider, true);

            GameObject go = Instantiate(spawnPrefab, pos, Quaternion.Euler(rot)) as GameObject;
            HurtBox _hurt = go.GetComponent<HurtBox>();

            force.Do(continuous, hit, _hurt);
            torque.Do(continuous, hit, _hurt, true);
        }
    }
}
