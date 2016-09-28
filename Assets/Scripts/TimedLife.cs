using UnityEngine;
using System.Collections;

public class TimedLife : MonoBehaviour {
	public float time;
	void Start () {
		Destroy (gameObject, time);
	}
}
