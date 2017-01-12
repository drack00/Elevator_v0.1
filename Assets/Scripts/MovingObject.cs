using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;

[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingObject : MonoBehaviour
{
    //components
    public Animator animator
    {
        get
        {
            return GetComponent<Animator>();
        }
    }
    public new Rigidbody rigidbody
    {
        get
        {
            return GetComponent<Rigidbody>();
        }
    }
    public new Collider collider
    {
        get
        {
            return GetComponent<Collider>();
        }
    }

    //directional stuff
    public virtual Vector3 GetFocus()
    {
        return transform.forward;
    }
    public virtual Vector2 GetInput()
    {
        return Vector2.zero;
    }
    public virtual void RotateView()
    {
        transform.rotation = Quaternion.LookRotation(GetFocus());
    }
    public virtual void NextAction() { }

    //clash functions
    public virtual void Clash()
    {
        animator.SetTrigger("Clash");
    }

    //blocking mask
    [System.Serializable]
    [System.Flags]
    public enum BlockingMask
    {
        Physics = Rigidbody | Gravity | Collision,
        Rigidbody = 1,
        Gravity = 2,
        Collision = 4,

        Input = Movement | Orientation | Action,
        Movement = 8,
        Orientation = 16,
        Action = 32,
    }
    [HideInInspector]
    [EnumFlag("Blocking Mask")]
    public BlockingMask blockingMask;

    //movement
    [System.Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 8.0f;   // Speed when walking forward
        public float BackwardSpeed = 4.0f;  // Speed when walking backwards
        public float StrafeSpeed = 4.0f;    // Speed when walking sideways
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
    public MovementSettings movementSettings = new MovementSettings();
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    private float m_YRotation;
    private Vector3 m_GroundContactNormal;
    private bool m_PreviouslyGrounded;
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
    private void GroundCheck()
    {
        m_PreviouslyGrounded = animator.GetBool("Grounded");

        Vector3 extents = collider.bounds.extents;
        float height = extents.y;
        extents = new Vector3(extents.x, 0.0f, extents.z);
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, extents.magnitude, Vector3.down, out hitInfo,
                               (height - extents.magnitude) + 
                               advancedSettings.groundCheckDistance))
        {
            animator.SetBool("Grounded", true);
            m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            animator.SetBool("Grounded", false);
            m_GroundContactNormal = Vector3.up;
        }
    }

    //grab functions
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
            Vector3 position = (Quaternion.LookRotation(GetFocus()) * grabOffset);
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

    //monobehaviour functions
    public virtual void Awake() { }
	public virtual void Start ()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = 0;
        alive = true;
        ResetHealth();
        ResetStun();
    }
	public virtual void Update ()
    {
        animator.SetFloat ("MoveSpeed", new Vector3(rigidbody.velocity.x, 0.0f, rigidbody.velocity.z).magnitude);

        if (animator.GetBool("Grounded"))
            StopGrabbing();

        if (health < maxHealth)
            health += healthRegen * Time.deltaTime;
        if (stunned)
            stun -= stunRecovery * Time.deltaTime;
    }
    public virtual void FixedUpdate()
    {
        if ((blockingMask & BlockingMask.Rigidbody) != 0 && !rigidbody.isKinematic)
            rigidbody.isKinematic = true;
        else if ((blockingMask & BlockingMask.Rigidbody) == 0 && rigidbody.isKinematic)
            rigidbody.isKinematic = false;
        if ((blockingMask & BlockingMask.Gravity) != 0 && rigidbody.useGravity)
            rigidbody.useGravity = false;
        else if ((blockingMask & BlockingMask.Gravity) == 0 && !rigidbody.useGravity)
            rigidbody.useGravity = true;
        if ((blockingMask & BlockingMask.Collision) != 0 && !collider.isTrigger)
            collider.isTrigger = true;
        else if ((blockingMask & BlockingMask.Collision) == 0 && collider.isTrigger)
            collider.isTrigger = false;

        NextAction();

        RotateView();

        GroundCheck();
        Vector2 input = GetInput();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || animator.GetBool("Grounded")))
        {
            Vector3 desiredMove = GetFocus() * input.y + (Quaternion.Euler(0.0f, 90.0f, 0.0f) * GetFocus()) * input.x;
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

        if (animator.GetBool("Grounded"))
        {
            rigidbody.drag = 5f;

            if (Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && rigidbody.velocity.magnitude < 1f)
            {
                rigidbody.Sleep();
            }
        }
        else
        {
            rigidbody.drag = 0f;
            if (m_PreviouslyGrounded)
            {
                StickToGroundHelper();
            }
        }
    }
}