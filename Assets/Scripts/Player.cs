using UnityEngine;
using System.Collections;
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

    public override Vector3 GetFocus()
    {
        return new Vector3(root.forward.x, 0f, root.forward.z).normalized;
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
        root.transform.rotation = cam.transform.rotation;

        //change active moveset
        if (_activeMoveSet != activeMoveSet && !lockActiveMoveSet)
            activeMoveSet = activeMoveSet;
    }
    public override void NextAction()
    {
        if ((blockingMask & BlockingMask.Action) != 0)
        {
            animator.ResetTrigger("Jump");

            return;
        }

        //moveset animator inputs
        activeMoveSet.SetPositiveInput(CrossPlatformInputManager.GetButtonDown("Fire1"), CrossPlatformInputManager.GetButtonDown("Fire2"));
        activeMoveSet.SetNegativeInput(CrossPlatformInputManager.GetButtonUp("Fire1"), CrossPlatformInputManager.GetButtonUp("Fire2"));
        activeMoveSet.ToggleHoldInput(CrossPlatformInputManager.GetButton("Fire1"), CrossPlatformInputManager.GetButton("Fire2"));

        //primary animator inputs
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
            animator.SetTrigger("JumpEdge");
        animator.SetBool("Jump", CrossPlatformInputManager.GetButton("Jump"));
    }

    public Camera cam;
    private float pitch = 0f;
    private float yaw = 0f;
    public float camSmoothing = 5f;
    public float xSensitivity = 2f;
    public float ySensitivity = 2f;

    [HideInInspector]
    public bool lockActiveMoveSet = false;
    [HideInInspector]
    public MoveSet[] moveSets;
	private MoveSet _activeMoveSet;
	public MoveSet activeMoveSet
    {
		get
        {
            if (lockActiveMoveSet)
                return _activeMoveSet;

			Quaternion[] _moveSets = new Quaternion[moveSets.Length];
			for (int i = 0; i < _moveSets.Length; i++)
            {
				_moveSets [i] = Quaternion.Euler(moveSets [i].xRotation, root.localRotation.y, root.localRotation.z);
			}
			return moveSets[(MathStuff.GetClosestRotationIndex (root.localRotation, _moveSets))];
		}
		set
        {
            if (lockActiveMoveSet)
                return;

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