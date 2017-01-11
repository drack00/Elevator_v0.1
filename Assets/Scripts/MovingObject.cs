using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingObject : MonoBehaviour
{
    #region stuff i lifted from rigidbody firstperson controller
    [System.Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 8.0f;   // Speed when walking forward
        public float BackwardSpeed = 4.0f;  // Speed when walking backwards
        public float StrafeSpeed = 4.0f;    // Speed when walking sideways
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector]
        public float CurrentTargetSpeed = 8f;

        public void UpdateDesiredTargetSpeed(Vector2 input)
        {
            if (input == Vector2.zero) return;
            if (input.x > 0 || input.x < 0)
            {
                //strafe
                CurrentTargetSpeed = StrafeSpeed;
            }
            if (input.y < 0)
            {
                //backwards
                CurrentTargetSpeed = BackwardSpeed;
            }
            if (input.y > 0)
            {
                //forwards
                //handled last as if strafing and moving forward at the same time forwards speed should take precedence
                CurrentTargetSpeed = ForwardSpeed;
            }
        }

    }

    [System.Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
        public float stickToGroundHelperDistance = 0.5f; // stops the character
        public float slowDownRate = 20f; // rate at which the controller comes to a stop when there is no input
        public bool airControl; // can the user control the direction that is being moved in the air
    }

    public Camera cam;
    public MovementSettings movementSettings = new MovementSettings();
    public MouseLook mouseLook = new MouseLook();
    public AdvancedSettings advancedSettings = new AdvancedSettings();

    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;

    public Vector3 Velocity
    {
        get { return rigidbody.velocity; }
    }

    public bool Grounded
    {
        get { return m_IsGrounded; }
    }

    public bool Jumping
    {
        get { return m_Jumping; }
    }

    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }

    private void StickToGroundHelper()
    {
        Vector3 extents = collider.bounds.extents;
        float height = extents.y;
        extents = new Vector3(extents.x, 0.0f, extents.z);
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, extents.magnitude, Vector3.down, out hitInfo,
                               (height - extents.magnitude) +
                               advancedSettings.stickToGroundHelperDistance))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rigidbody.velocity = Vector3.ProjectOnPlane(rigidbody.velocity, hitInfo.normal);
            }
        }
    }

    private Vector2 GetInput()
    {

        Vector2 input = new Vector2
        {
            x = CrossPlatformInputManager.GetAxis("Horizontal"),
            y = CrossPlatformInputManager.GetAxis("Vertical")
        };
        movementSettings.UpdateDesiredTargetSpeed(input);
        return input;
    }

    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        if (cam != null)
            mouseLook.LookRotation(transform, cam.transform);

        if (m_IsGrounded || advancedSettings.airControl)
        {
            // Rotate the rigidbody velocity to match the new direction that the character is looking
            Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
            rigidbody.velocity = velRotation * rigidbody.velocity;
        }
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        m_PreviouslyGrounded = m_IsGrounded;
        Vector3 extents = collider.bounds.extents;
        float height = extents.y;
        extents = new Vector3(extents.x, 0.0f, extents.z);
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, extents.magnitude, Vector3.down, out hitInfo,
                               (height - extents.magnitude) + advancedSettings.groundCheckDistance))
        {
            m_IsGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
        if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
        {
            m_Jumping = false;
        }
    }
    #endregion


    public Animator animator
    {
        get
        {
            return GetComponent<Animator> ();
        }
    }
	public new Rigidbody rigidbody
    {
		get
        {
			return GetComponent<Rigidbody> ();
		}
	}
    public new Collider collider
    {
        get
        {
            return GetComponent<Collider>();
        }
    }

    //blocking mask
    [System.Serializable]
    [System.Flags]
    public enum BlockingMask
    {
        Physics = Rigidbody | Gravity,
        Rigidbody = 1,
        Gravity = 2,

        Input = LeftInputs | RightInputs | Controller,
        LeftInputs = 16,
        RightInputs = 32,
        Controller = 64,

        AI = AI_Movement | AI_Orientation | AI_Action,
        AI_Movement = 128,
        AI_Orientation = 256,
        AI_Action = 512
    }
    [HideInInspector]
    [EnumFlag("Blocking Mask")]
    public BlockingMask blockingMask;

    //clash function
    public virtual void Clash ()
    {
        animator.SetTrigger("Clash");
    }

    //grab functions
    public virtual Vector3 GetFocusDirection()
    {
        if (cam != null)
            return cam.transform.forward;
        else
            return transform.forward;
    }
    public Vector3 grabOffset;
    private bool isGrabbing = false;
    public void StartGrabbing(MovingObject other, bool invertXOffset = false)
    {
        if (!isGrabbing)
            StartCoroutine(Grab(other, invertXOffset));
    }
    public void StopGrabbing()
    {
        if (isGrabbing)
            stopGrabbing = true;
    }
    private bool stopGrabbing = false;
    public IEnumerator Grab(MovingObject other, bool invertXOffset = false)
    {
        isGrabbing = true;

        animator.SetBool("Grab", true);
        other.animator.SetBool("Grabbed", true);

        stopGrabbing = false;
        while (!stopGrabbing)
        {
            Vector3 position = (Quaternion.LookRotation(GetFocusDirection()) * grabOffset);
            if (invertXOffset)
                position = new Vector3(-1 * position.x, position.y, position.z);
            position += transform.position;
            other.rigidbody.MovePosition(position);
            Vector3 faceDir = (transform.position - other.rigidbody.position).normalized;
            other.rigidbody.MoveRotation(Quaternion.LookRotation(faceDir));

            yield return null;
        }
        stopGrabbing = false;

        animator.SetBool("Grab", false);
        other.animator.SetBool("Grabbed", false);

        isGrabbing = false;
    }

    //character stats
    private bool _alive;
	public bool alive
    {
		get
        {
			return _alive;
		}
		set
        {
			_alive = value;

			animator.SetBool ("Alive", _alive);
		}
	}
    public virtual void Kill ()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = 0;
        ResetStun();

        alive = false;
    }
    public virtual void Dead ()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = 0;
        ResetStun();

        gameObject.SetActive(false);
    }

	public float maxHealth;
	private float _health;
	public float health
    {
		get
        {
			return _health;
		}
		set
        {
			if (!alive)
				return;

			if (value > maxHealth)
				value = maxHealth;
			if (value < 0.0f)
				value = 0.0f;

			_health = value;

			animator.SetFloat ("Health", _health);

            if (Mathf.Approximately(_health, Mathf.Epsilon))
                Kill();
		}
	}
	public float healthRegen;
	public virtual void ResetHealth ()
    {
		health = maxHealth;
	}
		
	public bool stunned
    {
		get
        {
			return stun > 0.0f;
		}
	}
	public float maxStun;
	private float _stun;
	public float stun
    {
		get
        {
			return _stun;
		}
		set
        {
			if (!alive)
				return;

			if (value > maxStun)
				value = maxStun;
			if (value < 0.0f)
				value = 0.0f;

			_stun = value;

			animator.SetFloat ("Stun", _stun);
		}
	}
	public float stunRecovery;
	public virtual void ResetStun ()
    {
		stun = 0.0f;
	}

	public virtual void Spawn ()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = 0;
        alive = true;
		ResetHealth ();
		ResetStun ();
	}

    public virtual void Awake() { }

	public virtual void Start ()
    {
        if (cam != null)
            mouseLook.Init(transform, cam.transform);

        Spawn ();
	}

	public virtual void Update ()
    {
        /*if ((blockingMask & BlockingMask.Controller) != 0 && controller.enabled)
            controller.enabled = false;
        else if ((blockingMask & BlockingMask.Controller) == 0 && !controller.enabled)
            controller.enabled = true;*/
        if ((blockingMask & BlockingMask.Rigidbody) != 0 && !rigidbody.isKinematic)
            rigidbody.isKinematic = true;
        else if ((blockingMask & BlockingMask.Rigidbody) == 0 && rigidbody.isKinematic)
            rigidbody.isKinematic = false;

        RotateView();

        if (CrossPlatformInputManager.GetButtonDown("Jump") && !m_Jump)
        {
            m_Jump = true;
        }

        if (health < maxHealth)
			health += healthRegen * Time.deltaTime;
		if (stunned)
			stun -= stunRecovery * Time.deltaTime;
	}

    public virtual void FixedUpdate()
    {
        GroundCheck();
        Vector2 input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
        {
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = GetFocusDirection() * input.y + (Quaternion.Euler(0.0f, 90.0f, 0.0f) * GetFocusDirection()) * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
            if (rigidbody.velocity.sqrMagnitude <
                (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
            {
                rigidbody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
            }
        }

        if (m_IsGrounded)
        {
            rigidbody.drag = 5f;

            if (m_Jump)
            {
                rigidbody.drag = 0f;
                rigidbody.velocity = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
                rigidbody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                m_Jumping = true;
            }

            if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && rigidbody.velocity.magnitude < 1f)
            {
                rigidbody.Sleep();
            }
        }
        else
        {
            rigidbody.drag = 0f;
            if (m_PreviouslyGrounded && !m_Jumping)
            {
                StickToGroundHelper();
            }
        }
        m_Jump = false;
    }
}