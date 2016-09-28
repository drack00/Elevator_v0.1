using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour {
	void Update () {
		transform.rotation = Quaternion.LookRotation ((Camera.main.transform.position - transform.position).normalized);
	}
}
