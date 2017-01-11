using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MovingObject
{
    public Vector3[] moveSets;
	private Vector3 _activeMoveSet;
	public Vector3 activeMoveSet
    {
		get
        {
			Vector3[] _moveSets = new Vector3[moveSets.Length];
			for (int i = 0; i < _moveSets.Length; i++)
            {
				_moveSets [i] = moveSets [i];
			}
			return (MathStuff.GetClosestRotation (cam.transform.localRotation, _moveSets));
		}
		set
        {
			_activeMoveSet = value;

			if (_activeMoveSet == moveSets [0])
				animator.SetInteger ("ActiveMoveSet", 0);
			else 
				animator.SetInteger ("ActiveMoveSet", 1);
		}
	}
    public SyncPosition[] syncPositions;

	public float swiftChangeMoveSetSpeed;
	private bool swiftChangeMoveSet;

	public override void Start ()
    {
		base.Start ();

		activeMoveSet = moveSets[0];
	}

	public override void Update ()
    {
        //update base class
        base.Update ();

		//swift change lerp, and slow change correction
		if (swiftChangeMoveSet)
        {
			cam.transform.localRotation = Quaternion.Lerp (cam.transform.localRotation, Quaternion.Euler (_activeMoveSet), swiftChangeMoveSetSpeed * Time.deltaTime);
			mouseLook.Init (transform, cam.transform);
			if (cam.transform.localRotation == Quaternion.Euler (_activeMoveSet))
				swiftChangeMoveSet = false;
		}
        else if (_activeMoveSet != activeMoveSet)
			activeMoveSet = activeMoveSet;

		//align ui gizmos
        foreach(SyncPosition syncPos in syncPositions)
        {
            syncPos.enabled = activeMoveSet == moveSets[1] && !Grounded;
        }

		//controller animation
		animator.SetBool ("Grounded", Grounded);
        Vector3 velocity = Velocity;
        velocity = new Vector3(velocity.x, 0.0f, velocity.z);
        animator.SetFloat ("MoveSpeed", velocity.magnitude);

        //can't grab while grounded
        if (Grounded)
            StopGrabbing();

        //mouse wheel inputs
        if (CrossPlatformInputManager.GetAxis ("Mouse ScrollWheel") < 0.0f)
        {
			swiftChangeMoveSet = true;
			activeMoveSet = moveSets [1];
		}
		if (CrossPlatformInputManager.GetAxis ("Mouse ScrollWheel") > 0.0f)
        {
			swiftChangeMoveSet = true;
			activeMoveSet = moveSets [0];
		}
			
		//mouse button inputs
		animator.SetBool ("Left", CrossPlatformInputManager.GetButton ("Fire1"));
		animator.SetBool ("Right", CrossPlatformInputManager.GetButton ("Fire2"));

        //mouse button edges
        if ((blockingMask & BlockingMask.LeftInputs) == 0)
        {
            if (CrossPlatformInputManager.GetButtonDown("Fire1"))
                animator.SetTrigger("LeftEdge");
        }
        else
            animator.SetBool("Left", false);
        if ((blockingMask & BlockingMask.RightInputs) == 0)
        {
            if (CrossPlatformInputManager.GetButtonDown("Fire2"))
                animator.SetTrigger("RightEdge");
        }
        else
            animator.SetBool("Right", false);
	}
}