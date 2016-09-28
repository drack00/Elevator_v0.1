using UnityEngine;
using System.Collections;

public class AnimationBehaviour : MonoBehaviour {
	public new Rigidbody rigidbody;

	[System.Serializable]
	public enum ApplyMethod {
		None, Instant, Continuous
	}
	[System.Serializable]
	public enum ApplyType {
		Absolute, 

		RelativeThis, 
		RelativeThisRigidbody, 

		ThisToThisRigidbody, 

		ThisRigidbodyToThis
	}
	[System.Serializable]
	public struct AnimationPhase {
		//torque
		public ApplyMethod torqueMethod;
		public ApplyType torqueType;
		public Vector3 torque;

		//force
		public ApplyMethod forceMethod;
		public ApplyType forceType;
		public Vector3 force;
	}
	public AnimationPhase onEnable;
	public AnimationPhase onUpdate;
	public AnimationPhase onDisable;

	//controls
	public bool blockMovement;
	public bool blockLeftInputs;
	public bool blockRightInputs;

	[System.Serializable]
	public enum UpdateOn {
		None, FixedUpdate, Update, LateUpdate
	}
	public UpdateOn updateOn;

	public Vector3 GetCorrectVector (ApplyType applyType, Vector3 vector) {
		Vector3 _vector = Vector3.zero;

		switch (applyType) {

		case ApplyType.Absolute:
			_vector = vector;
			break;

		case ApplyType.RelativeThis:
			_vector = transform.TransformDirection (vector);
			break;
		case ApplyType.RelativeThisRigidbody:
			_vector = rigidbody.transform.TransformDirection (vector);
			break;

		case ApplyType.ThisToThisRigidbody:
			_vector = Quaternion.LookRotation ((rigidbody.transform.position - transform.position).normalized) * vector;
			break;

		case ApplyType.ThisRigidbodyToThis:
			_vector = Quaternion.LookRotation ((transform.position - rigidbody.transform.position).normalized) * vector;
			break;
		}

		return _vector;
	}

	void DoPhase (AnimationPhase phase, float timeDelta) {
		//torque
		if (phase.torqueMethod == ApplyMethod.Instant)
			MathStuff.ImpulseTorque (rigidbody, GetCorrectVector (phase.torqueType, phase.torque));
		else if (phase.torqueMethod == ApplyMethod.Continuous)
			rigidbody.AddTorque (GetCorrectVector (phase.torqueType, phase.torque * timeDelta));

		//force
		if (phase.forceMethod == ApplyMethod.Instant)
			MathStuff.ImpulseForce (rigidbody, GetCorrectVector (phase.forceType, phase.force));
		else if (phase.forceMethod == ApplyMethod.Continuous)
			rigidbody.AddForce (GetCorrectVector (phase.forceType, phase.force * timeDelta));
	}

	void OnEnable () {
		DoPhase (onEnable, 1.0f);

		//controls
		if(rigidbody.GetComponent<Player> () != null) {
			if (blockMovement)
				rigidbody.GetComponent<Player> ().controller.enabled = false;
			if (blockLeftInputs)
				rigidbody.GetComponent<Player> ().blockLeftInputs = true;
			if (blockRightInputs)
				rigidbody.GetComponent<Player> ().blockRightInputs = true;
		}
	}

	void FixedUpdate () {
		if (updateOn == UpdateOn.FixedUpdate)
			DoUpdate (Time.fixedDeltaTime);
	}
	void Update () {
		if (updateOn == UpdateOn.Update)
			DoUpdate (Time.deltaTime);
	}
	void LateUpdate () {
		if (updateOn == UpdateOn.LateUpdate)
			DoUpdate (Time.deltaTime);
	}
	void DoUpdate (float timeDelta) {
		DoPhase (onUpdate, timeDelta);
	}

	void OnDisable () {
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