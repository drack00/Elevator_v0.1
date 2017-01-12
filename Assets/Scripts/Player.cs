using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MovingObject
{
    public override Vector3 GetFocus()
    {
        return cam.transform.forward; ;
    }
    public override Vector2 GetInput()
    {
        if ((blockingMask & BlockingMask.Movement) != 0)
            return base.GetInput();

        Vector2 input = new Vector2
        {
            x = CrossPlatformInputManager.GetAxis("Horizontal"),
            y = CrossPlatformInputManager.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }
    public override void RotateView()
    {
        if ((blockingMask & BlockingMask.Orientation) != 0)
            return;

        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        //look rotation
        mouseLook.LookRotation(transform, cam.transform);

        if (animator.GetBool("Grounded") || advancedSettings.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            rigidbody.velocity = velRotation * rigidbody.velocity;
        }
    }
    public override void NextAction()
    {
        if ((blockingMask & BlockingMask.Action) != 0)
        {
            animator.SetBool("Left", false);
            animator.SetBool("Right", false);

            return;
        }
        else
        {
            animator.SetBool("Left", CrossPlatformInputManager.GetButton("Fire1"));
            animator.SetBool("Right", CrossPlatformInputManager.GetButton("Fire2"));
        } 

        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            animator.SetTrigger("LeftEdge");
        if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            animator.SetTrigger("RightEdge");
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
            animator.SetTrigger("Jump");
    }

    public Camera cam;
    public MouseLook mouseLook = new MouseLook();

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
	private bool swiftChangeMoveSet = false;

    public override void Start ()
    {
        mouseLook.Init(transform, cam.transform);

        activeMoveSet = moveSets[0];

        base.Start ();
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
            syncPos.enabled = activeMoveSet == moveSets[1] && !animator.GetBool("Grounded");
        }

        //mouse wheel
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
	}
}