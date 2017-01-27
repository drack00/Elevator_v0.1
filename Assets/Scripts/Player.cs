using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class Player : AnimatedMovingObject
{
    [System.Serializable]
    public class MoveSet
    {
        public float xRotation;
        public UIGizmo[] gizmos;
    }

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
        {
            foreach(MoveSet moveSet in moveSets)
            {
                foreach (UIGizmo gizmo in moveSet.gizmos)
                {
                    gizmo.enabled = false;
                }
            }

            return;
        }

        foreach (MoveSet moveSet in moveSets)
        {
            foreach (UIGizmo gizmo in moveSet.gizmos)
            {
                gizmo.enabled = true;
            }
        }
    }
    public override void NextAction()
    {
        if ((blockingMask & BlockingMask.Action) != 0)
        {
            animator.ResetTrigger("LeftEdge");
            animator.ResetTrigger("RightEdge");

            animator.ResetTrigger("Jump");

            return;
        }

        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
            animator.SetTrigger("LeftEdge");
        if (CrossPlatformInputManager.GetButtonDown("Fire2"))
            animator.SetTrigger("RightEdge");

        animator.SetBool("Left", CrossPlatformInputManager.GetButton("Fire1"));
        animator.SetBool("Right", CrossPlatformInputManager.GetButton("Fire2"));

        if (CrossPlatformInputManager.GetButtonDown("Jump"))
            animator.SetTrigger("JumpEdge");

        animator.SetBool("Jump", CrossPlatformInputManager.GetButton("Jump"));
    }

    public Camera cam;
    public MouseLook mouseLook = new MouseLook();

    public MoveSet[] moveSets;
	private MoveSet _activeMoveSet;
	public MoveSet activeMoveSet
    {
		get
        {
			Quaternion[] _moveSets = new Quaternion[moveSets.Length];
			for (int i = 0; i < _moveSets.Length; i++)
            {
				_moveSets [i] = Quaternion.Euler(moveSets [i].xRotation, cam.transform.localRotation.y, cam.transform.localRotation.z);
			}
			return moveSets[(MathStuff.GetClosestRotationIndex (cam.transform.localRotation, _moveSets))];
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
    private Quaternion currentCamRot = Quaternion.identity;
    private Quaternion desiredCamRot = Quaternion.identity;
    private float swiftChangeMoveSetSpeed = 10.0f;
    private float _swiftChangeMoveSetSpeed = 0.0f;
    private bool swiftChangeMoveSet = false;
    private bool m_PreviousSwiftChangeMoveSet = false;

    public override void Start ()
    {
        mouseLook.Init(cam.transform);

        activeMoveSet = moveSets[0];

        base.Start ();
	}
	public override void Update ()
    {
        //update base class
        base.Update ();

        m_PreviousSwiftChangeMoveSet = swiftChangeMoveSet;

        //mouse wheel
        if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0.0f)
        {
            swiftChangeMoveSet = true;
            activeMoveSet = moveSets[0];
        }
        if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0.0f)
        {
            swiftChangeMoveSet = true;
            activeMoveSet = moveSets[1];
        }

        //swift change lerp, and slow change correction
        if (swiftChangeMoveSet)
        {
            //
            if(!m_PreviousSwiftChangeMoveSet)
            {
                currentCamRot = cam.transform.localRotation;
                desiredCamRot = Quaternion.Euler(_activeMoveSet.xRotation, currentCamRot.eulerAngles.y, currentCamRot.eulerAngles.z);
                _swiftChangeMoveSetSpeed = 0.0f;
            }

            //
            _swiftChangeMoveSetSpeed += swiftChangeMoveSetSpeed * Time.deltaTime;
            if (_swiftChangeMoveSetSpeed > 1.0f)
                _swiftChangeMoveSetSpeed = 1.0f;

            //
            cam.transform.localRotation = Quaternion.Lerp(currentCamRot, desiredCamRot, _swiftChangeMoveSetSpeed);
            mouseLook.Init(cam.transform);

            //
            if (Mathf.Approximately(_swiftChangeMoveSetSpeed, 1.0f))
                swiftChangeMoveSet = false;
		}
        else if (_activeMoveSet != activeMoveSet)
			activeMoveSet = activeMoveSet;

        //align ui gizmos
        foreach (MoveSet moveSet in moveSets)
        {
            foreach (UIGizmo gizmo in moveSet.gizmos)
            {
                gizmo.restrictY = activeMoveSet != moveSet;
            }
        }
	}
    public override void FixedUpdate()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        //look rotation
        mouseLook.LookRotation(cam.transform);

        base.FixedUpdate();
    }
}