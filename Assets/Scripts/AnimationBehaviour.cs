using UnityEngine;
using System.Collections;

public class AnimationBehaviour : MonoBehaviour
{
	public new Rigidbody rigidbody;

    [System.Serializable]
    public struct ApplyMovement
    {
        public bool additive;
        public MathStuff.SortVectors.ApplyType type;
        public Vector3 amount;

        public void Do(AnimationBehaviour ab, float multiplier, bool isTorque = false)
        {
            if (!isTorque)
            {
                Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, ab, amount);
                Vector3 force = vector * multiplier;
                if (additive)
                    ab.rigidbody.AddForce(force);
                else
                    ab.rigidbody.velocity = force;
            }
            else
            {
                Vector3 vector = MathStuff.SortVectors.GetCorrectVector(type, ab, amount, true);
                Vector3 torque = vector * multiplier;
                if (additive)
                    ab.rigidbody.AddTorque(torque);
                else
                    ab.rigidbody.angularVelocity = torque;
            }
        }
    }
    [System.Serializable]
	public struct AnimationPhase
    {
		public ApplyMovement torque;
		public ApplyMovement force;

        public void Do(AnimationBehaviour ab, float multiplier)
        {
            torque.Do(ab, multiplier, true);
            force.Do(ab, multiplier);
        }
	}
	public AnimationPhase onEnable;
	public AnimationPhase onUpdate;
	public AnimationPhase onDisable;

    [System.Flags]
    [System.Serializable]
    public enum Blocking
    {
        LeftInputs = 0x0001,
        RightInputs = 0x0002,
        Controller = 0x0004,
        Rigidbody = 0x0008
    }
    public Blocking blocking;

    [System.Serializable]
	public enum UpdateOn
    {
		None, FixedUpdate, Update, LateUpdate
	}
	public UpdateOn updateOn;

    void DoPhase (AnimationPhase phase, float timeDelta)
    {
        phase.Do(this, timeDelta);
    }

	void OnEnable ()
    {
		DoPhase (onEnable, 1.0f);

		//controls
		if(rigidbody.GetComponent<Player> () != null)
        {
            rigidbody.GetComponent<Player> ().blocking += (int)blocking;
		}
	}

	void FixedUpdate ()
    {
		if (updateOn == UpdateOn.FixedUpdate)
			DoUpdate (Time.fixedDeltaTime);
	}
	void Update ()
    {
		if (updateOn == UpdateOn.Update)
			DoUpdate (Time.deltaTime);
	}
	void LateUpdate ()
    {
		if (updateOn == UpdateOn.LateUpdate)
			DoUpdate (Time.deltaTime);
	}
	void DoUpdate (float timeDelta)
    {
		DoPhase (onUpdate, timeDelta);
	}

	void OnDisable ()
    {
		DoPhase (onDisable, 1.0f);

		//controls
		if(rigidbody.GetComponent<Player> () != null)
        {
            rigidbody.GetComponent<Player> ().blocking -= (int)blocking;
		}
	}
}