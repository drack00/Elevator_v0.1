﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Hit : MonoBehaviour
{
	public new Collider collider
    {
		get
        {
			return GetComponent<Collider> ();
		}
	}

    [System.Serializable]
    public enum TimeScaleModifier
    {
        None, Duration, UntilExit
    }
    public static Hurt timeScaler;
    public static bool onTimeScale = false;
	public static float defaultTimeScale;
	public static IEnumerator OnTimeScale (float timeScale, float timeScaleDuration)
    {
		onTimeScale = true;

		Time.timeScale = timeScale;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		float _releaseTimeScaleDuration = 0.0f;

		while (_releaseTimeScaleDuration < timeScaleDuration) {
			_releaseTimeScaleDuration += Time.deltaTime;

			yield return null;
		}

		Time.timeScale = defaultTimeScale;
		Time.fixedDeltaTime = 0.02f * Time.timeScale;

		onTimeScale = false;
	}

    [System.Serializable]
    public struct ApplyMovement
    {
        public bool continuous;
        public bool additive;
        public MathStuff.SortVectors.ApplyType type;
        public Vector3 amount;

        public void Do(Hit hit, Hurt hurt, bool refContinuous, bool isTorque = false)
        {
            if (refContinuous == continuous)
            {
                if (continuous)
                {
                    if (!isTorque)
                    {
                        Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, hit, hurt, amount);
                        float multiplier = !continuous ? hurt.forceInstant.GetAngleInstant() : hurt.forceContinuous.GetAngleContinuous(Time.deltaTime);
                        Vector3 force = vector * multiplier;
                        if (additive)
                            hit.collider.attachedRigidbody.AddForce(force, ForceMode.VelocityChange);
                        else
                            hit.collider.attachedRigidbody.velocity = force;
                    }
                    else
                    {
                        Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, hit, hurt, amount, true);
                        float multiplier = !continuous ? hurt.torqueInstant.GetAngleInstant() : hurt.torqueContinuous.GetAngleContinuous(Time.deltaTime);
                        Vector3 torque = vector * multiplier;
                        if (additive)
                            hit.collider.attachedRigidbody.AddTorque(torque, ForceMode.VelocityChange);
                        else
                            hit.collider.attachedRigidbody.angularVelocity = torque;
                    }
                }
                else
                {
                    if (!isTorque)
                    {
                        Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, hit, hurt, amount);
                        float multiplier = !continuous ? 1.0f : Time.deltaTime;
                        Vector3 force = vector * multiplier;
                        if (additive)
                            hurt.collider.attachedRigidbody.AddForce(force, ForceMode.VelocityChange);
                        else
                            hurt.collider.attachedRigidbody.velocity = force;
                    }
                    else
                    {
                        Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, hit, hurt, amount, true);
                        float multiplier = !continuous ? 1.0f : Time.deltaTime;
                        Vector3 torque = vector * multiplier;
                        if (additive)
                            hurt.collider.attachedRigidbody.AddTorque(torque, ForceMode.VelocityChange);
                        else
                            hurt.collider.attachedRigidbody.angularVelocity = torque;
                    }
                }
            }
        }
    }
    [System.Serializable]
    public struct ApplyStat
    {
        public bool continuous;
        public bool additive;
        public float amount;

        public float Do(bool refContinuous, float refAmount)
        {
            if (continuous == refContinuous)
            {
                if(continuous)
                    return refAmount + amount;
                else
                    return amount;
            }

            return refAmount;
        }
    }

    [System.Serializable]
	public struct Spawn
    {
		public bool continuous;
		public GameObject spawnPrefab;

		public MathStuff.SortVectors.ApplyType positionType;
		public Vector3 position;

		public MathStuff.SortVectors.ApplyType rotationType;
		public Vector3 rotation;

		public ApplyMovement torque;
		public ApplyMovement force;

        public void Do(Hit hit, Hurt hurt, bool refContinuous)
        {
            if (continuous == refContinuous)
            {
                Vector3 pos = MathStuff.SortVectors.GetCorrectVector(positionType, hit, hurt, position);
                Vector3 rot = MathStuff.SortVectors.GetCorrectVector(rotationType, hit, hurt, rotation, true);

                GameObject go = Instantiate(spawnPrefab, pos, Quaternion.Euler(rot)) as GameObject;
                Hurt _hurt = go.GetComponent<Hurt>();

                torque.Do(hit, _hurt, refContinuous, true);
                force.Do(hit, _hurt, refContinuous);
            }
        }
	}

    public int maxHurts;
    public bool retainHurts;

    [System.Serializable]
    public class Behaviour
    {
        public ApplyMovement torque;
        public ApplyMovement force;

        public ApplyStat damage;
        public ApplyStat stun;

        public Spawn[] spawns;

        public virtual void Do(Hit hit, Hurt hurt, bool continuous)
        {
            torque.Do(hit, hurt, continuous, true);
            force.Do(hit, hurt, continuous);

            if (hurt.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
            {
                MovingObject mo = hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>();

                mo.health = damage.Do(continuous, mo.health);
                mo.stun = stun.Do(continuous, mo.stun);
            }

            foreach (Spawn spawn in spawns)
            {
                spawn.Do(hit, hurt, continuous);
            }
        }
    }
	[System.Serializable]
	public class HitBehaviour : Behaviour
    {
        public override void Do(Hit hit, Hurt hurt, bool continuous)
        {
            if (hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
            {
                if (hurt.willClash && continuous == hurt.continuousClash)
                {
                    hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>().Clash();
                    if (hurt.overrideHit) return;
                }
            }

            base.Do(hit, hurt, continuous);
        }
    }
	public HitBehaviour hitBehaviour;
	[System.Serializable]
	public class ResponseBehaviour : Behaviour
    {
		public TimeScaleModifier timeScaleModifier;
		public float timeScale;
		public float timeScaleDuration;

        public override void Do(Hit hit, Hurt hurt, bool continous)
        {
            if (timeScaleModifier == TimeScaleModifier.UntilExit && timeScaler == null)
            { 
                Time.timeScale = timeScale;
                timeScaler = hurt;
            }
            else if (timeScaleModifier == TimeScaleModifier.Duration && !onTimeScale)
                hit.StartCoroutine(OnTimeScale(timeScale, timeScaleDuration));

            base.Do(hit, hurt, continous);
        }
    }
	public ResponseBehaviour responseBehaviour;

	public LayerMask targetLayers;
		
	private List<Hurt> hurts;
	void Awake ()
    {
		defaultTimeScale = Time.timeScale;

        hurts = new List<Hurt> ();
	}
	void OnEnable ()
    {
        hurts = new List<Hurt> ();
	}
	void OnDisable ()
    {
        hurts = new List<Hurt> ();
	}
		
	void OnTriggerEnter (Collider other)
    {
		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0 || other.GetComponent<Hurt>() == null)
			return;
		
		Hurt hurt = other.GetComponent<Hurt>();
		
		if (hurts.Count < maxHurts)
        {
            hurts.Add (hurt);

            responseBehaviour.Do(this, hurt, false);
            hitBehaviour.Do(this, hurt, false);
		}
	}

	void OnTriggerStay (Collider other)
    {
		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0 || other.GetComponent<Hurt>() == null)
			return;
		
		Hurt hurt = other.GetComponent<Hurt>();

		bool otherValid = false;
		foreach (Hurt _hurt in hurts)
        {
			if (hurt == _hurt)
            {
				otherValid = true;
				break;
			}
		}

		if (otherValid)
        {
            responseBehaviour.Do(this, hurt, true);
            hitBehaviour.Do(this, hurt, true);
        }
	}

	void OnTriggerExit (Collider other)
    {
		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0 || other.GetComponent<Hurt>() == null)
			return;
		
		Hurt hurt = other.GetComponent<Hurt>();

        if (responseBehaviour.timeScaleModifier == TimeScaleModifier.UntilExit && timeScaler == hurt)
        {
            Time.timeScale = defaultTimeScale;
            timeScaler = null;
        }

        if (!retainHurts)
            hurts.Remove(hurt);
	}
}