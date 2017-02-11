using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public class Player : AnimatedMovingObject
{
    public override void SetGrounded(bool _grounded)
    {
        base.SetGrounded(_grounded);

        foreach (MoveSet moveSet in moveSets)
        {
            moveSet.ToggleGrounded(_grounded);
        }
    }
    private Vector2 wallDirection = Vector2.zero;
    public override void SetWallDirection(Vector2 _wallDirection)
    {
        wallDirection = _wallDirection;
        (moveSets[0] as BareKnuckle).ToggleWalled(_wallDirection);
    }
    public override Vector2 GetWallDirection()
    {
        return wallDirection;
    }
    private bool grab = false;
    public override void SetGrab(bool _grab)
    {
        grab = _grab;
        (moveSets[0] as BareKnuckle).ToggleGrab(_grab);
    }
    public override bool GetGrab()
    {
        return grab;
    }
    private Vector2 speed = Vector2.zero;
    public override void SetSpeed(Vector2 _speed)
    {
        speed = _speed;
        (moveSets[1] as FleetFoot).ToggleMovement(_speed);
    }
    public override Vector2 GetSpeed()
    {
        return speed;
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
        input.Normalize();

        SetSpeed(input);

        movementSettings.UpdateDesiredTargetSpeed(input);

        return input;
    }
    public override void RotateView()
    {
        if ((blockingMask & BlockingMask.Orientation) != 0)
            return;

        //recenter camera
        root.transform.position = cam.transform.position;
        root.transform.rotation = cam.transform.rotation;

        //change active moveset
        if (_activeMoveSet != activeMoveSet)
            activeMoveSet = activeMoveSet;

        //align psuedo-moveset and toggle gizmos
        psuedoMoveSet.Align(root);
    }
    public override void NextAction()
    {
        if ((blockingMask & BlockingMask.Action) != 0)
        {
            foreach(MoveSet moveSet in moveSets)
            {
                moveSet.Reset();
            }

            animator.ResetTrigger("Jump");
            animator.SetBool("Jump", false);

            return;
        }

        //primary animator inputs
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
            animator.SetTrigger("JumpEdge");
        animator.SetBool("Jump", CrossPlatformInputManager.GetButton("Jump"));

        //get relevent movesets
        MoveSet dualActiveMoveSet = activeMoveSet;
        MoveSet leftActiveMoveSet = activeMoveSet;
        MoveSet rightActiveMoveSet = activeMoveSet;
        foreach (MoveSet moveSet in moveSets)
        {
            if (dualActiveMoveSet != moveSet && moveSet.activeInputs == MoveSet.ActiveInputs.Dual)
                dualActiveMoveSet = moveSet;
            if (leftActiveMoveSet != moveSet && moveSet.activeInputs == MoveSet.ActiveInputs.Left)
                leftActiveMoveSet = moveSet;
            if (rightActiveMoveSet != moveSet && moveSet.activeInputs == MoveSet.ActiveInputs.Right)
                rightActiveMoveSet = moveSet;
        }

        //generate psuedo-moveset and set inputs
        if (leftActiveMoveSet == rightActiveMoveSet)
            psuedoMoveSet = new PsuedoMoveSet(dualActiveMoveSet);
        else
            psuedoMoveSet = new PsuedoMoveSet(leftActiveMoveSet, rightActiveMoveSet);
        psuedoMoveSet.Do();
    }

    private class PsuedoMoveSet
    {
        private Animator[] allOtherAnimators
        {
            get
            {
                List<Animator> _allOtherAnimators = new List<Animator>(MoveSet.GetMoveSet(leftAnimator).transform.parent.GetComponentsInChildren<Animator>());

                if (dualAnimator != null && _allOtherAnimators.Contains(dualAnimator))
                    _allOtherAnimators.Remove(dualAnimator);
                if (_allOtherAnimators.Contains(leftAnimator))
                    _allOtherAnimators.Remove(leftAnimator);
                if (_allOtherAnimators.Contains(rightAnimator))
                    _allOtherAnimators.Remove(rightAnimator);

                return _allOtherAnimators.ToArray();
            }
        }

        Animator dualAnimator = null, leftAnimator, rightAnimator;

        public PsuedoMoveSet(MoveSet _moveSet)
        {
            dualAnimator = _moveSet.dualAnimator;
            leftAnimator = _moveSet.leftAnimator;
            rightAnimator = _moveSet.rightAnimator;
        }
        public PsuedoMoveSet(MoveSet _leftMoveSet, MoveSet _rightMoveSet)
        {
            leftAnimator = _leftMoveSet.leftAnimator;
            rightAnimator = _rightMoveSet.rightAnimator;
        }

        public void Align(Transform root)
        {
            //align displayed animators
            if (dualAnimator != null)
            {
                if (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
                {
                    dualAnimator.transform.position = root.position;
                    dualAnimator.transform.rotation = root.rotation;
                }
            }
            if (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            {
                leftAnimator.transform.position = root.position;
                leftAnimator.transform.rotation = root.rotation;
            }
            if (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            {
                rightAnimator.transform.position = root.position;
                rightAnimator.transform.rotation = root.rotation;
            }
        }

        public void Do()
        {
            //get raw inputs
            bool leftPositive = CrossPlatformInputManager.GetButtonDown("Fire1");
            bool rightPositive = CrossPlatformInputManager.GetButtonDown("Fire2");
            bool leftNegative = CrossPlatformInputManager.GetButtonUp("Fire1");
            bool rightNegative = CrossPlatformInputManager.GetButtonUp("Fire2");
            bool leftHold = CrossPlatformInputManager.GetButton("Fire1");
            bool rightHold = CrossPlatformInputManager.GetButton("Fire2");

            //check for dual inputs
            bool dualPositive = 
                dualAnimator != null &&
                leftPositive && rightPositive &&
                dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
                leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") && rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest");
            bool dualNegative =
                dualAnimator != null &&
                leftNegative && rightNegative &&
                dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge") &&
                leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") && rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest");
            bool dualHold =
                dualAnimator != null &&
                leftHold && rightHold && (
                (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") && (
                    ((leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")) || 
                    ((rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))) ||
                ((dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                    (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") && rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))));

            //set active animator
            bool isDualActive = dualPositive || dualNegative || dualHold || (dualAnimator != null && !dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"));
            if (dualAnimator != null)
                dualAnimator.SetBool("Active", isDualActive);
            leftAnimator.SetBool("Active", !isDualActive);
            rightAnimator.SetBool("Active", !isDualActive);

            //set relevent inputs
            if (dualPositive)
            {
                dualAnimator.SetTrigger("Positive");
            }
            else if(!dualHold)
            {
                if (leftPositive &&
                    leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
                    (dualAnimator == null || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))
                    leftAnimator.SetTrigger("Positive");
                if (rightPositive &&
                    rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
                    (dualAnimator == null || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))
                    rightAnimator.SetTrigger("Positive");
            }
            if (dualNegative)
            {
                dualAnimator.SetTrigger("Negative");
            }
            else if (!dualHold)
            {
                if (leftNegative &&
                    leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge") &&
                    (dualAnimator == null || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))
                    leftAnimator.SetTrigger("Negative");
                if (rightNegative &&
                    rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge") &&
                    (dualAnimator == null || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))
                    rightAnimator.SetTrigger("Negative");
            }
            if (dualHold)
            {
                dualAnimator.SetBool("Hold", true);

                leftAnimator.SetBool("Hold", false);
                rightAnimator.SetBool("Hold", false);
            }
            else
            {
                if (dualAnimator != null)
                    dualAnimator.SetBool("Hold", false);

                leftAnimator.SetBool("Hold",
                    leftHold && (
                    (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
                        (dualAnimator != null && dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))) ||
                    ((leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        (dualAnimator == null || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))));
                rightAnimator.SetBool("Hold",
                    rightHold && (
                    (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
                        (dualAnimator != null && dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))) ||
                    ((rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        (dualAnimator == null || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")))));
            }

            //reset all other moveset animators
            foreach (Animator otherAnimator in allOtherAnimators)
            {
                otherAnimator.SetBool("Active", false);
                otherAnimator.ResetTrigger("Positive");
                otherAnimator.ResetTrigger("Negative");
                otherAnimator.SetBool("Hold", false);
            }
        }
    }
    private PsuedoMoveSet psuedoMoveSet;

    public Camera cam;
    private float pitch = 0f;
    private float yaw = 0f;
    public float camSmoothing = 5f;
    public float xSensitivity = 2f;
    public float ySensitivity = 2f;

    [HideInInspector]
    public MoveSet[] moveSets;
	private MoveSet _activeMoveSet = null;
	public MoveSet activeMoveSet
    {
		get
        {
			Quaternion[] _moveSets = new Quaternion[moveSets.Length];
			for (int i = 0; i < _moveSets.Length; i++)
            {
				_moveSets [i] = Quaternion.Euler(moveSets [i].xRotation, root.localRotation.y, root.localRotation.z);
			}
			return moveSets[(MathStuff.GetClosestRotationIndex (root.localRotation, _moveSets))];
		}
		set
        {
			_activeMoveSet = value;
		}
	}

    public override void Awake()
    {
        moveSets = GetComponentsInChildren<MoveSet>();

        base.Awake();
    }
    public override void Start ()
    {
        activeMoveSet = moveSets[0];

        base.Start ();
	}
	public override void Update ()
    {
        //update base class
        base.Update ();

        //dont continue if time scale is approximitly 0
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        //mouse wheel
        if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") > 0.0f)
            pitch = moveSets[0].xRotation;
        if (CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") < 0.0f)
            pitch = moveSets[1].xRotation;

        //set new camera rotation
        yaw += CrossPlatformInputManager.GetAxis("Mouse X") * xSensitivity;
        pitch -= CrossPlatformInputManager.GetAxis("Mouse Y") * ySensitivity;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);

        //
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);
        //Quaternion inverseTargetRot = Quaternion.Euler(-1*pitch, yaw, 180f);

        //
        cam.transform.localRotation = //Quaternion.Angle(cam.transform.localRotation, targetRot) < Quaternion.Angle(cam.transform.localRotation, inverseTargetRot) ?
            Quaternion.Lerp(cam.transform.localRotation, targetRot, camSmoothing * Time.deltaTime); //:
            //Quaternion.Lerp(cam.transform.localRotation, inverseTargetRot, camSmoothing * Time.deltaTime);
    }
}