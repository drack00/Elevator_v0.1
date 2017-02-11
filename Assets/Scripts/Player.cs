using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public class Player : AnimatedMovingObject
{
    public override void SetGrounded(bool _grounded)
    {
        base.SetGrounded(_grounded);
        activeMoveSet.ToggleGrounded(_grounded);
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

        //


        //generate psuedo-moveset
        if (leftActiveMoveSet == rightActiveMoveSet)
            psuedoMoveSet = new PsuedoMoveSet(dualActiveMoveSet);
        else
            psuedoMoveSet = new PsuedoMoveSet(leftActiveMoveSet, rightActiveMoveSet);
        psuedoMoveSet.Do();

        //primary animator inputs
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
            animator.SetTrigger("JumpEdge");
        animator.SetBool("Jump", CrossPlatformInputManager.GetButton("Jump"));
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
        private GameObject[] allOtherGizmos
        {
            get
            {
                List<GameObject> _allOtherGizmos = new List<GameObject>();

                foreach (MoveSet moveSet in MoveSet.allMoveSets)
                {
                    _allOtherGizmos.Add(moveSet.dualGizmo0);
                    _allOtherGizmos.Add(moveSet.dualGizmo1);
                    _allOtherGizmos.Add(moveSet.leftGizmo);
                    _allOtherGizmos.Add(moveSet.rightGizmo);
                }

                if (_allOtherGizmos.Contains(dualGizmo0))
                    _allOtherGizmos.Remove(dualGizmo0);
                if (_allOtherGizmos.Contains(dualGizmo1))
                    _allOtherGizmos.Remove(dualGizmo1);
                if (_allOtherGizmos.Contains(leftGizmo))
                    _allOtherGizmos.Remove(leftGizmo);
                if (_allOtherGizmos.Contains(rightGizmo))
                    _allOtherGizmos.Remove(rightGizmo);

                return _allOtherGizmos.ToArray();
            }
        }

        Animator dualAnimator = null, leftAnimator, rightAnimator;
        GameObject dualGizmo0 = null, dualGizmo1 = null, leftGizmo, rightGizmo;

        public PsuedoMoveSet(MoveSet _moveSet)
        {
            dualAnimator = _moveSet.dualAnimator;
            leftAnimator = _moveSet.leftAnimator;
            rightAnimator = _moveSet.rightAnimator;

            dualGizmo0 = _moveSet.dualGizmo0;
            dualGizmo1 = _moveSet.dualGizmo1;
            leftGizmo = _moveSet.leftGizmo;
            rightGizmo = _moveSet.rightGizmo;
        }
        public PsuedoMoveSet(MoveSet _leftMoveSet, MoveSet _rightMoveSet)
        {
            leftAnimator = _leftMoveSet.leftAnimator;
            rightAnimator = _rightMoveSet.rightAnimator;

            leftGizmo = _leftMoveSet.leftGizmo;
            rightGizmo = _rightMoveSet.rightGizmo;
        }

        public void AlignAndToggle(Transform root)
        {
            //enable displayed gizmos, and align displayed animators
            if (dualAnimator != null)
            {
                dualAnimator.transform.position = root.position;
                dualAnimator.transform.rotation = root.rotation;
                dualGizmo0.SetActive(MoveSet.GetMoveSet(dualAnimator).activeInputs == MoveSet.ActiveInputs.Dual);
                dualGizmo1.SetActive(MoveSet.GetMoveSet(dualAnimator).activeInputs == MoveSet.ActiveInputs.Dual);
            }
            leftAnimator.transform.position = root.position;
            leftAnimator.transform.rotation = root.rotation;
            leftGizmo.SetActive((dualAnimator == null || !dualGizmo0.activeSelf));
            rightAnimator.transform.position = root.position;
            rightAnimator.transform.rotation = root.rotation;
            rightGizmo.SetActive((dualAnimator == null || !dualGizmo1.activeSelf));

            //disable other gizmos
            foreach(GameObject gizmo in allOtherGizmos)
            {
                gizmo.SetActive(false);
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

            foreach (Animator otherAnimator in allOtherAnimators)
            {
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

			foreach(MoveSet moveSet in moveSets)
            {
                moveSet.ToggleActive(moveSet == _activeMoveSet);
            }
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

        //
        psuedoMoveSet.AlignAndToggle(root);

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