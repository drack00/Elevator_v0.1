using UnityEngine;
using System.Collections;

public class ProjectileSpawner : MonoBehaviour {
	public GameObject projectilePrefab;

	public bool torqueRelative;
	public Vector3 torque;
	public bool forceRelative;
	public Vector3 force;

	[System.Flags]
	[System.Serializable]
	public enum SpawnOn {
		Enable = 1, Disable = 2, FixedUpdate = 3, Update = 4, LateUpdate = 5
	}
	public SpawnOn spawnOn;
	public float spawnDelay;
	private float _spawnDelay;

	void Start () {
		_spawnDelay = spawnDelay;
	}

	void DoSpawn () {
		GameObject go = Instantiate (projectilePrefab, transform.position, transform.rotation) as GameObject;

		Vector3 _torque = !torqueRelative ? torque : transform.TransformDirection (torque);
		Vector3 _force = !forceRelative ? force : transform.TransformDirection (force);

		go.GetComponent<Rigidbody> ().AddTorque (_torque);
		go.GetComponent<Rigidbody> ().AddForce (_force);
	}

	void OnEnable () {
		if (spawnOn == SpawnOn.Enable)
			DoSpawn ();
	}

	void FixedUpdate () {
		if (spawnOn == SpawnOn.FixedUpdate)
			DoUpdate (Time.fixedDeltaTime);
	}
	void Update () {
		if (spawnOn == SpawnOn.Update)
			DoUpdate (Time.deltaTime);
	}
	void LateUpdate () {
		if (spawnOn == SpawnOn.LateUpdate)
			DoUpdate (Time.deltaTime);
	}
	void DoUpdate (float timeDelta) {
		_spawnDelay -= timeDelta;

		if (_spawnDelay <= 0.0f) {
			_spawnDelay = spawnDelay;
			DoSpawn ();
		}
	}

	void OnDisable () {
		if (spawnOn == SpawnOn.Disable)
			DoSpawn ();
	}
}