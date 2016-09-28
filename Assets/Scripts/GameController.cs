using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {
	void Awake () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}