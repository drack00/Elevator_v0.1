﻿using UnityEngine;
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

            if (!isTorque)
            {
                Vector3 vector = SortVectors.GetCorrectVector(type, flatten, amount,
                    hit.transform.position, hit.transform.rotation, hit.mo.transform.position, Quaternion.LookRotation(hit.mo.GetFocus()),
                    hurt.transform.position, hurt.transform.rotation, hurt.mo.transform.position, Quaternion.LookRotation(hurt.mo.GetFocus())) * multiplier;

                if (additive)
                    hurt.rigidbody.AddForce(vector, ForceMode.VelocityChange);
                else
                    hurt.rigidbody.velocity = vector;
            }
            else
            {
                Vector3 vector = (SortVectors.GetCorrectVector(type, flatten, Quaternion.Euler(amount),
                    hit.transform.position, hit.transform.rotation, hit.mo.transform.position, Quaternion.LookRotation(hit.mo.GetFocus()),
                    hurt.transform.position, hurt.transform.rotation, hurt.mo.transform.position, Quaternion.LookRotation(hurt.mo.GetFocus()))).eulerAngles * multiplier;

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
            int i = 0;
            while(i < gos.Count)
            {
                GameObject _go = gos[i];
                if (_go == null)
                    gos.Remove(_go);
                else
                    i++;
            }

            if (gos.Count >= maxSpawns && maxSpawns >= 0)
                return;

            Vector3 pos = SortVectors.GetCorrectVector(positionType, positionFlatten, position,
                hit.transform.position, hit.transform.rotation, hit.mo.transform.position, Quaternion.LookRotation(hit.mo.GetFocus()),
                hurt.transform.position, hurt.transform.rotation, hurt.mo.transform.position, Quaternion.LookRotation(hurt.mo.GetFocus()),
                false);
            Quaternion rot = SortVectors.GetCorrectVector(rotationType, rotationFlatten, Quaternion.Euler(rotation),
                hit.transform.position, hit.transform.rotation, hit.mo.transform.position, Quaternion.LookRotation(hit.mo.GetFocus()),
                hurt.transform.position, hurt.transform.rotation, hurt.mo.transform.position, Quaternion.LookRotation(hurt.mo.GetFocus()),
                false);

            GameObject go = Instantiate(spawnPrefab, pos, rot) as GameObject;
            gos.Add(go);
            HitBox _hit = go.GetComponent<HitBox>();
            _hit.exclude.Add(hit.mo);
            HurtBox _hurt = go.GetComponent<HurtBox>();

            force.Do(continuous, hit, _hurt);
            torque.Do(continuous, hit, _hurt, true);
        }
    }

    public virtual void Awake()
    {
        rigidbody = collider.attachedRigidbody;
        mo = rigidbody.GetComponent<MovingObject>();
    }
}
