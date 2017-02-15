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
    private Vector3 wallDirection = Vector3.zero;
    public override void SetWallDirection(Vector3 _wallDirection)
    {
        wallDirection = _wallDirection;
        animator.SetBool("Walled", _wallDirection != Vector3.zero);
        (moveSets[0] as BareKnuckle).ToggleWalled(Quaternion.Inverse(Quaternion.LookRotation(GetFocus())) * _wallDirection);
    }
    public override Vector3 GetWallDirection()
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
    public override void SetSpeed(Vector2 speed)
    {
        base.SetSpeed(speed);
        (moveSets[1] as FleetFoot).ToggleMovement(speed);
    }

    public override Vector2 GetInput()
    {
        if ((blockingMask & BlockingMask.Movement) != 0)
            return base.GetInput();

        //movement inputs
        Vector2 input = new Vector2
        {
            x = CrossPlatformInputManager.GetAxis("Horizontal"),
            y = CrossPlatformInputManager.GetAxis("Vertical")
        };
        input.Normalize();
        SetSpeed(input);
        movementSettings.UpdateDesiredTargetSpeed(input);

        //movement actions
        if (CrossPlatformInputManager.GetButtonDown("Jump") && aerialAction)
        {
            animator.SetTrigger("Jump");
            if (!GetGrounded() && GetWallDirection() == Vector3.zero && !GetCapped())
                aerialAction = false;
        }

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
    private bool aerialAction = true;
    public override void NextAction()
    {
        if ((blockingMask & BlockingMask.Action) != 0)
        {
            animator.ResetTrigger("Jump");

            foreach (MoveSet moveSet in moveSets)
            {
                moveSet.Reset();
            }

            return;
        }

        //get relevent movesets
        MoveSet dualActiveMoveSet = activeMoveSet;
        MoveSet leftActiveMoveSet = activeMoveSet;
        MoveSet rightActiveMoveSet = activeMoveSet;
        foreach (MoveSet moveSet in moveSets)
        {
            if (moveSet != activeMoveSet)
            {
                if (moveSet.activeInputs == MoveSet.ActiveInputs.Dual)
                    dualActiveMoveSet = moveSet;
                if (moveSet.activeInputs == MoveSet.ActiveInputs.Left)
                    leftActiveMoveSet = moveSet;
                if (moveSet.activeInputs == MoveSet.ActiveInputs.Right)
                    rightActiveMoveSet = moveSet;
            }
        }
        if (dualActiveMoveSet.canCancelInputs == MoveSet.ActiveInputs.Dual)
            dualActiveMoveSet = activeMoveSet;
        if (leftActiveMoveSet.canCancelInputs == MoveSet.ActiveInputs.Left)
            leftActiveMoveSet = activeMoveSet;
        if (rightActiveMoveSet.canCancelInputs == MoveSet.ActiveInputs.Right)
            rightActiveMoveSet = activeMoveSet;

        //generate psuedo-moveset and set inputs
        if (leftActiveMoveSet == rightActiveMoveSet)
            psuedoMoveSet = new PsuedoMoveSet(dualActiveMoveSet);
        else
            psuedoMoveSet = new PsuedoMoveSet(leftActiveMoveSet, rightActiveMoveSet);
        if (psuedoMoveSet.Do() && !GetGrounded() && GetWallDirection() == Vector3.zero && !GetCapped())
            aerialAction = false;
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
                if (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
                {
                    dualAnimator.transform.position = root.position;
                    dualAnimator.transform.rotation = root.rotation;
                }
            }
            if (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            {
                leftAnimator.transform.position = root.position;
                leftAnimator.transform.rotation = root.rotation;
            }
            if (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            {
                rightAnimator.transform.position = root.position;
                rightAnimator.transform.rotation = root.rotation;
            }
        }

        public bool Do()
        {
            bool _do = false;

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
                (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) &&
                (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) && (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"));
            bool dualNegative =
                dualAnimator != null &&
                leftNegative && rightNegative &&
                dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge") &&
                (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) && (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"));
            bool dualHold =
                dualAnimator != null &&
                leftHold && rightHold && (
                ((dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) && (
                    ((leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))) || 
                    ((rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))) ||
                ((dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                    ((leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) && (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")))));

            //set active animator
            bool isDualActive = dualAnimator != null && !dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest");
            if (dualAnimator != null)
                dualAnimator.SetBool("Active", isDualActive);
            leftAnimator.SetBool("Active", !isDualActive);
            rightAnimator.SetBool("Active", !isDualActive);

            //set relevent inputs
            if (dualPositive)
            {
                dualAnimator.SetTrigger("Positive");
                _do = true;
            }
            else if(!dualHold)
            {
                if (leftPositive &&
                    (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) &&
                    (dualAnimator == null || (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))
                {
                    leftAnimator.SetTrigger("Positive");
                    _do = true;
                }
                if (rightPositive &&
                    (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) &&
                    (dualAnimator == null || (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))
                {
                    rightAnimator.SetTrigger("Positive");
                    _do = true;
                }
            }
            if (dualNegative)
            {
                dualAnimator.SetTrigger("Negative");
                _do = true;
            }
            else if (!dualHold)
            {
                if (leftNegative &&
                    leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge") &&
                    (dualAnimator == null || (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))
                {
                    leftAnimator.SetTrigger("Negative");
                    _do = true;
                }
                if (rightNegative &&
                    rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge") &&
                    (dualAnimator == null || (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))
                {
                    rightAnimator.SetTrigger("Negative");
                    _do = true;
                }
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
                    ((leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) &&
                        (dualAnimator != null && dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))) ||
                    ((leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        (dualAnimator == null || (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))));
                rightAnimator.SetBool("Hold",
                    rightHold && (
                    ((rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1")) &&
                        (dualAnimator != null && dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))) ||
                    ((rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge")) &&
                        (dualAnimator == null || (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest")|| dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"))))));
            }

            //reset all other moveset animators
            foreach (Animator otherAnimator in allOtherAnimators)
            {
                otherAnimator.SetBool("Active", !otherAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") && !otherAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest1"));
                otherAnimator.ResetTrigger("Positive");
                otherAnimator.ResetTrigger("Negative");
                otherAnimator.SetBool("Hold", false);
            }

            return _do;
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
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (GetGrounded() || GetWallDirection() != Vector3.zero || GetCapped())
            aerialAction = true;
    }
}