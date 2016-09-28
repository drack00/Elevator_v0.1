using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hit : MonoBehaviour {
	public new Collider collider {
		get {
			return GetComponent<Collider> ();
		}
	}

	public static bool onTimeScale = false;
	public static float defaultTimeScale;
	public static IEnumerator OnTimeScale (float timeScale, float timeScaleDuration) {
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
	public enum ApplyMethod {
		None, Instant, Continuous
	}
	[System.Serializable]
	public enum ApplyType {
		Absolute, 

		RelativeThis, 
		RelativeThisRigidbody, 
		RelativeOther, 
		RelativeOtherRigidbody, 

		ThisToOther, 
		ThisToThisRigidbody, 
		ThisToOtherRigidbody, 

		ThisRigidbodyToThis, 
		ThisRigidbodyToOther,
		ThisRigidbodyToOtherRigidbody,

		OtherToThis, 
		OtherToThisRigidbody,
		OtherToOtherRigidbody,

		OtherRigidbodyToThis, 
		OtherRigidbodyToOther,
		OtherRigidbodyToThisRigidbody
	}
	[System.Serializable]
	public struct SpawnBehaviour {
		public ApplyMethod spawnMethod;
		public GameObject spawnPrefab;

		[System.Serializable]
		public enum VectorType {
			Absolute,

			RelativeThis, 
			RelativeThisRigidbody, 
			RelativeOther, 
			RelativeOtherRigidbody,
		}
		public VectorType positionType;
		public Vector3 position;

		public VectorType rotationType;
		public Vector3 rotation;

		public ApplyMethod torqueMethod;
		public ApplyType torqueType;
		public Vector3 torque;

		public ApplyMethod forceMethod;
		public ApplyType forceType;
		public Vector3 force;
	}
	[System.Serializable]
	public enum TimeScaleModifier {
		None, Duration, UntilExit
	}
	[System.Serializable]
	public struct HitBehaviour {
		public int maxResponses;

		public ApplyMethod torqueMethod;
		public ApplyType torqueType;
		public Vector3 torque;

		public ApplyMethod forceMethod;
		public ApplyType forceType;
		public Vector3 force;

		public ApplyMethod damageMethod;
		public float damage;

		public ApplyMethod stunMethod;
		public float stun;

		public SpawnBehaviour[] spawns;
	}
	public HitBehaviour hitBehaviour;
	[System.Serializable]
	public struct ResponseBehaviour {
		public ApplyMethod torqueMethod;
		public ApplyType torqueType;
		public Vector3 torque;

		public ApplyMethod forceMethod;
		public ApplyType forceType;
		public Vector3 force;

		public ApplyMethod damageMethod;
		public float damage;

		public ApplyMethod stunMethod;
		public float stun;

		public SpawnBehaviour[] spawns;

		public TimeScaleModifier timeScaleModifier;
		public float timeScale;
		public float timeScaleDuration;
	}
	public ResponseBehaviour responseBehaviour;

	public LayerMask targetLayers;
		
	private List<Hurt> responses;
	void Awake () {
		defaultTimeScale = Time.timeScale;

		responses = new List<Hurt> ();
	}
	void OnEnable () {
		responses = new List<Hurt> ();
	}
	void OnDisable () {
		responses = new List<Hurt> ();
	}

	public Vector3 GetCorrectVector (ApplyType applyType, Hurt other, Vector3 vector) {
		Vector3 _vector = Vector3.zero;

		switch (applyType) {

		case ApplyType.Absolute:
			_vector = vector;
			break;

		case ApplyType.RelativeThis:
			_vector = transform.TransformDirection (vector);
			break;
		case ApplyType.RelativeThisRigidbody:
			_vector = collider.attachedRigidbody.transform.TransformDirection (vector);
			break;
		case ApplyType.RelativeOther:
			_vector = other.transform.TransformDirection (vector);
			break;
		case ApplyType.RelativeOtherRigidbody:
			_vector = other.collider.attachedRigidbody.transform.TransformDirection (vector);
			break;

		case ApplyType.ThisToThisRigidbody:
			_vector = Quaternion.LookRotation ((collider.attachedRigidbody.transform.position - transform.position).normalized) * vector;
			break;
		case ApplyType.ThisToOther:
			_vector = Quaternion.LookRotation ((other.transform.position - transform.position).normalized) * vector;
			break;
		case ApplyType.ThisToOtherRigidbody:
			_vector = Quaternion.LookRotation ((other.collider.attachedRigidbody.transform.position - transform.position).normalized) * vector;
			break;

		case ApplyType.ThisRigidbodyToThis:
			_vector = Quaternion.LookRotation ((transform.position - collider.attachedRigidbody.transform.position).normalized) * vector;
			break;
		case ApplyType.ThisRigidbodyToOther:
			_vector = Quaternion.LookRotation ((other.transform.position - collider.attachedRigidbody.transform.position).normalized) * vector;
			break;
		case ApplyType.ThisRigidbodyToOtherRigidbody:
			_vector = Quaternion.LookRotation ((other.collider.attachedRigidbody.transform.position - collider.attachedRigidbody.transform.position).normalized) * vector;
			break;

		case ApplyType.OtherToThis:
			_vector = Quaternion.LookRotation ((transform.position - other.transform.position).normalized) * vector;
			break;
		case ApplyType.OtherToThisRigidbody:
			_vector = Quaternion.LookRotation ((collider.attachedRigidbody.transform.position - other.transform.position).normalized) * vector;
			break;
		case ApplyType.OtherToOtherRigidbody:
			_vector = Quaternion.LookRotation ((other.collider.attachedRigidbody.transform.position - other.transform.position).normalized) * vector;
			break;

		case ApplyType.OtherRigidbodyToThis:
			_vector = Quaternion.LookRotation ((transform.position - other.collider.attachedRigidbody.transform.position).normalized) * vector;
			break;
		case ApplyType.OtherRigidbodyToThisRigidbody:
			_vector = Quaternion.LookRotation ((collider.attachedRigidbody.transform.position - other.collider.attachedRigidbody.transform.position).normalized) * vector;
			break;
		case ApplyType.OtherRigidbodyToOther:
			_vector = Quaternion.LookRotation ((other.transform.position - other.collider.attachedRigidbody.transform.position).normalized) * vector;
			break;
		}

		return _vector;
	}

	public Vector3 GetCorrectPosition (SpawnBehaviour.VectorType vectorType, Hurt other, Vector3 position) {
		Vector3 _position = Vector3.zero;

		switch (vectorType){

		case SpawnBehaviour.VectorType.Absolute:
			_position = position;
			break;

		case SpawnBehaviour.VectorType.RelativeOther:
			_position = other.transform.TransformPoint (position);
			break;
		case SpawnBehaviour.VectorType.RelativeOtherRigidbody:
			_position = other.collider.attachedRigidbody.transform.TransformPoint (position);
			break;

		case SpawnBehaviour.VectorType.RelativeThis:
			_position = transform.TransformPoint (position);
			break;
		case SpawnBehaviour.VectorType.RelativeThisRigidbody:
			_position = collider.attachedRigidbody.transform.TransformPoint (position);
			break;
		}

		return _position;
	}
	public Vector3 GetCorrectRotation (SpawnBehaviour.VectorType vectorType, Hurt other, Vector3 rotation) {
		Vector3 _rotation = Vector3.zero;

		switch (vectorType){

		case SpawnBehaviour.VectorType.Absolute:
			_rotation = rotation;
			break;

		case SpawnBehaviour.VectorType.RelativeOther:
			_rotation = other.transform.rotation.eulerAngles + rotation;
			break;
		case SpawnBehaviour.VectorType.RelativeOtherRigidbody:
			_rotation = other.collider.attachedRigidbody.transform.rotation.eulerAngles + rotation;
			break;

		case SpawnBehaviour.VectorType.RelativeThis:
			_rotation = transform.rotation.eulerAngles + rotation;
			break;
		case SpawnBehaviour.VectorType.RelativeThisRigidbody:
			_rotation = collider.attachedRigidbody.transform.rotation.eulerAngles + rotation;
			break;
		}

		return _rotation;
	}
		
	void OnTriggerEnter (Collider _other) {
		if (_other.GetComponent<Hurt> () == null)
			return;
		
		Hurt other = _other.GetComponent<Hurt> ();

		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0)
			return;
		
		if (responses.Count < hitBehaviour.maxResponses) {
			responses.Add (other);

			//response effects
			if (collider.attachedRigidbody != null) {
				if (collider.attachedRigidbody.gameObject.GetComponent<MovingObject> () != null) {
					if (other.willClash == ApplyMethod.Instant) {
						collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().Clash ();
						return;
					}
				}

				if (responseBehaviour.torqueMethod == ApplyMethod.Instant)
					MathStuff.ImpulseTorque (collider.attachedRigidbody, GetCorrectVector (responseBehaviour.torqueType, other, responseBehaviour.torque * other.torqueInstant.GetAngleInstant ()));
				if (responseBehaviour.forceMethod == ApplyMethod.Instant)
					MathStuff.ImpulseForce (collider.attachedRigidbody, GetCorrectVector (responseBehaviour.forceType, other, responseBehaviour.force * other.forceInstant.GetAngleInstant ()));
				if (collider.attachedRigidbody.gameObject.GetComponent<MovingObject> () != null) {
					if (responseBehaviour.stunMethod == ApplyMethod.Instant)
						collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().stun += responseBehaviour.stun;
				}

				foreach (SpawnBehaviour spawn in responseBehaviour.spawns) {
					if (spawn.spawnMethod == ApplyMethod.Instant)
						Instantiate (spawn.spawnPrefab, GetCorrectPosition (spawn.positionType, other, spawn.position), Quaternion.Euler (GetCorrectRotation (spawn.rotationType, other, spawn.rotation)));
				}
			}
			if (responseBehaviour.timeScaleModifier == TimeScaleModifier.UntilExit)
				Time.timeScale = responseBehaviour.timeScale;
			else if (responseBehaviour.timeScaleModifier == TimeScaleModifier.Duration && !onTimeScale)
				StartCoroutine (OnTimeScale (responseBehaviour.timeScale, responseBehaviour.timeScaleDuration));

			//hit effects
			if (other.collider.attachedRigidbody != null) {
				if (hitBehaviour.torqueMethod == ApplyMethod.Instant)
					MathStuff.ImpulseTorque (other.collider.attachedRigidbody, GetCorrectVector (hitBehaviour.torqueType, other, hitBehaviour.torque));
				if (hitBehaviour.forceMethod == ApplyMethod.Instant)
					MathStuff.ImpulseForce (other.collider.attachedRigidbody, GetCorrectVector (hitBehaviour.forceType, other, hitBehaviour.force));
				
				if (other.collider.attachedRigidbody.gameObject.GetComponent<MovingObject> () != null) {
					if (hitBehaviour.damageMethod == ApplyMethod.Instant)
						other.collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().health -= hitBehaviour.damage;
					if (hitBehaviour.stunMethod == ApplyMethod.Instant)
						other.collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().stun += hitBehaviour.stun;
				}

				foreach (SpawnBehaviour spawn in hitBehaviour.spawns) {
					if (spawn.spawnMethod == ApplyMethod.Instant)
						Instantiate (spawn.spawnPrefab, GetCorrectPosition (spawn.positionType, other, spawn.position), Quaternion.Euler (GetCorrectRotation (spawn.rotationType, other, spawn.rotation)));
				}
			}
		}
	}

	void OnTriggerStay (Collider _other) {
		if (_other.GetComponent<Hurt> () == null)
			return;
		
		Hurt other = _other.GetComponent<Hurt> ();

		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0)
			return;

		bool otherValid = false;
		foreach (Hurt response in responses) {
			if (response == other) {
				otherValid = true;
				break;
			}
		}

		if (otherValid) {
			//response effects
			if (collider.attachedRigidbody != null) {
				if (collider.attachedRigidbody.gameObject.GetComponent<MovingObject> () != null) {
					if (other.willClash == ApplyMethod.Continuous) {
						collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().Clash ();
						return;
					}
				}

				if (responseBehaviour.torqueMethod == ApplyMethod.Continuous)
					collider.attachedRigidbody.AddTorque (GetCorrectVector (responseBehaviour.torqueType, other, responseBehaviour.torque * other.torqueContinuous.GetAngleContinuous (Time.deltaTime)));
				if (responseBehaviour.forceMethod == ApplyMethod.Continuous)
					collider.attachedRigidbody.AddForce (GetCorrectVector (responseBehaviour.forceType, other, responseBehaviour.force * other.forceContinuous.GetAngleContinuous (Time.deltaTime)));

				if (collider.attachedRigidbody.gameObject.GetComponent<MovingObject> () != null) {
					if (responseBehaviour.stunMethod == ApplyMethod.Continuous)
						collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().stun += responseBehaviour.stun * Time.deltaTime;
				}

				foreach (SpawnBehaviour spawn in responseBehaviour.spawns) {
					if (spawn.spawnMethod == ApplyMethod.Continuous)
						Instantiate (spawn.spawnPrefab, GetCorrectPosition (spawn.positionType, other, spawn.position), Quaternion.Euler (GetCorrectRotation (spawn.rotationType, other, spawn.rotation)));
				}
			}

			//hit effects
			if (other.collider.attachedRigidbody != null) {
				if (hitBehaviour.torqueMethod == ApplyMethod.Continuous)
					other.collider.attachedRigidbody.AddTorque (GetCorrectVector (hitBehaviour.torqueType, other, hitBehaviour.torque * Time.deltaTime));
				if (hitBehaviour.forceMethod == ApplyMethod.Continuous)
					other.collider.attachedRigidbody.AddForce (GetCorrectVector (hitBehaviour.forceType, other, hitBehaviour.force * Time.deltaTime));
				
				if (other.collider.attachedRigidbody.gameObject.GetComponent<MovingObject> () != null) {
					if (hitBehaviour.damageMethod == ApplyMethod.Continuous)
						other.collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().health -= hitBehaviour.damage * Time.deltaTime;
					if (hitBehaviour.stunMethod == ApplyMethod.Instant)
						other.collider.attachedRigidbody.gameObject.GetComponent<MovingObject> ().stun += hitBehaviour.stun * Time.deltaTime;
				}

				foreach (SpawnBehaviour spawn in hitBehaviour.spawns) {
					if (spawn.spawnMethod == ApplyMethod.Continuous)
						Instantiate (spawn.spawnPrefab, GetCorrectPosition (spawn.positionType, other, spawn.position), Quaternion.Euler (GetCorrectRotation (spawn.rotationType, other, spawn.rotation)));
				}
			}
		}
	}

	void OnTriggerExit (Collider _other) {
		if (_other.GetComponent<Hurt> () == null)
			return;
		
		Hurt other = _other.GetComponent<Hurt> ();

		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0)
			return;

		if (responseBehaviour.timeScaleModifier == TimeScaleModifier.UntilExit)
			Time.timeScale = defaultTimeScale;

		responses.Remove (other);
	}
}