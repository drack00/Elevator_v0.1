﻿using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent (typeof(RigidbodyFirstPersonController))]
public class Player : MovingObject {
	[HideInInspector]public RigidbodyFirstPersonController controller {
		get {
			return GetComponent<RigidbodyFirstPersonController> ();
		}
	}
		
	public Vector3[] moveSets;
	private Vector3 _activeMoveSet;
	public Vector3 activeMoveSet {
		get {
			Vector3[] _moveSets = new Vector3[moveSets.Length];
			for (int i = 0; i < _moveSets.Length; i++) {
				_moveSets [i] = moveSets [i];
			}
			return (MathStuff.GetClosestRotation (controller.cam.transform.localRotation, _moveSets));
		}
		set {
			_activeMoveSet = value;

			if (_activeMoveSet == moveSets [0])
				animator.SetInteger ("ActiveMoveSet", 0);
			else 
				animator.SetInteger ("ActiveMoveSet", 1);
		}
	}
	private Vector3 inactiveMoveSet {
		get {
			if (activeMoveSet == moveSets [0])
				return moveSets [1];
			else
				return moveSets [0];
		}
	}

	public float swiftChangeMoveSetSpeed;
	private bool swiftChangeMoveSet;

	public GameObject[] moveSetsContent;
	private GameObject activeMoveSetContent {
		get {
			if (activeMoveSet == moveSets [0])
				return moveSetsContent [0];
			else
				return moveSetsContent [1];
		}
	}
	private GameObject inactiveMoveSetContent {
		get {
			if (inactiveMoveSet == moveSets [0])
				return moveSetsContent [0];
			else
				return moveSetsContent [1];
		}
	}

	public override void Clash () {
		animator.SetTrigger ("Clash");
	}

	public override void Start () {
		base.Start ();

		activeMoveSet = moveSets[0];

		blockLeftInputs = false;
		blockRightInputs = false;
	}

	[HideInInspector]public bool blockLeftInputs;
	[HideInInspector]public bool blockRightInputs;

	public override void Update () {
		base.Update ();

		//swift change lerp, and slow change correction
		if (swiftChangeMoveSet) {
			controller.cam.transform.localRotation = Quaternion.Lerp (controller.cam.transform.localRotation, Quaternion.Euler (_activeMoveSet), swiftChangeMoveSetSpeed * Time.deltaTime);
			controller.mouseLook.Init (transform, controller.cam.transform);
			if (controller.cam.transform.localRotation == Quaternion.Euler (_activeMoveSet))
				swiftChangeMoveSet = false;
		} else if (_activeMoveSet != activeMoveSet)
			activeMoveSet = activeMoveSet;

		//align ui gizmos
		if (activeMoveSetContent != moveSetsContent [1] || !controller.Grounded) {
			activeMoveSetContent.transform.localRotation = controller.cam.transform.localRotation;
			inactiveMoveSetContent.transform.localRotation = Quaternion.Euler (inactiveMoveSet);
		} else
			moveSetsContent [1].transform.localRotation = Quaternion.Euler (moveSets [1]);

		//controller animation
		animator.SetBool ("Grounded", controller.Grounded);
		animator.SetFloat ("MoveSpeed", controller.Velocity.magnitude);

		//mouse wheel inputs
		if (CrossPlatformInputManager.GetAxis ("Mouse ScrollWheel") < 0.0f) {
			swiftChangeMoveSet = true;
			activeMoveSet = moveSets [1];
		}
		if (CrossPlatformInputManager.GetAxis ("Mouse ScrollWheel") > 0.0f) {
			swiftChangeMoveSet = true;
			activeMoveSet = moveSets [0];
		}
			
		//mouse button inputs
		animator.SetBool ("Left", CrossPlatformInputManager.GetButton ("Fire1"));
		animator.SetBool ("Right", CrossPlatformInputManager.GetButton ("Fire2"));

		//mouse button edges
		if (!blockLeftInputs && CrossPlatformInputManager.GetButtonDown ("Fire1"))
			animator.SetTrigger ("LeftEdge");
		if (!blockRightInputs && CrossPlatformInputManager.GetButtonDown ("Fire2"))
			animator.SetTrigger ("RightEdge");
	}
}