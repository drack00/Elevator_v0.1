using UnityEngine;
using System.Collections;

public class AnimationBehaviour : MonoBehaviour {
	public new Rigidbody rigidbody;

    [System.Serializable]
    public enum ApplyType
    {
        Absolute,

        RelativeThis,
        RelativeThisRigidbody,
        ThisToThisRigidbody,
        ThisRigidbodyToThis
    }

    [System.Serializable]
    public struct ApplyMovement
    {
        public bool additive;
        public ApplyType type;
        public Vector3 amount;

        public void Do(AnimationBehaviour ab, float multiplier, bool isTorque = false)
        {
            if (!isTorque)
            {
                Vector3 vector = GetCorrectVector(type, ab, amount);
                Vector3 force = vector * multiplier;
                if (additive)
                    ab.rigidbody.AddForce(force);
                else
                    ab.rigidbody.velocity = force;
            }
            else
            {
                Vector3 vector = GetCorrectVector(type, ab, amount, true);
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

	//controls
	public bool blockMovement;
	public bool blockLeftInputs;
	public bool blockRightInputs;

	[System.Serializable]
	public enum UpdateOn
    {
		None, FixedUpdate, Update, LateUpdate
	}
	public UpdateOn updateOn;

    public static Vector3 GetCorrectVector(ApplyType applyType, AnimationBehaviour ab, Vector3 vector, bool rotation = false)
    {
        if (!rotation)
        {
            Vector3 _vector = Vector3.zero;

            switch (applyType)
            {

                case ApplyType.Absolute:
                    _vector = vector;
                    break;

                case ApplyType.RelativeThis:
                    _vector = ab.transform.TransformDirection(vector);
                    break;
                case ApplyType.RelativeThisRigidbody:
                    _vector = ab.rigidbody.transform.TransformDirection(vector);
                    break;
                case ApplyType.ThisToThisRigidbody:
                    _vector = Quaternion.LookRotation((ab.rigidbody.transform.position - ab.transform.position).normalized) * vector;
                    break;
                case ApplyType.ThisRigidbodyToThis:
                    _vector = Quaternion.LookRotation((ab.transform.position - ab.rigidbody.transform.position).normalized) * vector;
                    break;
            }

            return _vector;

        }
        else
        {
            Vector3 _rotation = Vector3.zero;

            switch (applyType)
            {

                case ApplyType.Absolute:
                    _rotation = vector;
                    break;

                case ApplyType.RelativeThis:
                    _rotation = ab.transform.rotation.eulerAngles + vector;
                    break;
                case ApplyType.RelativeThisRigidbody:
                    _rotation = ab.rigidbody.transform.rotation.eulerAngles + vector;
                    break;
            }

            return _rotation;
        }
    }

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
			if (blockMovement)
				rigidbody.GetComponent<Player> ().controller.enabled = false;
			if (blockLeftInputs)
				rigidbody.GetComponent<Player> ().blockLeftInputs = true;
			if (blockRightInputs)
				rigidbody.GetComponent<Player> ().blockRightInputs = true;
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
		if(rigidbody.GetComponent<Player> () != null) {
			if (blockMovement)
				rigidbody.GetComponent<Player> ().controller.enabled = true;
			if (blockLeftInputs)
				rigidbody.GetComponent<Player> ().blockLeftInputs = false;
			if (blockRightInputs)
				rigidbody.GetComponent<Player> ().blockRightInputs = false;
		}
	}
}