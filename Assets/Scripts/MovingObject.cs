using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.AI;

[RequireComponent (typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MovingObject : MonoBehaviour
{
    //components
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

    //functionality overrides
    public virtual Vector3 GetFocus()
    {
        return root.forward;
    }
    public virtual Vector2 GetInput()
    {
        SetSpeed(Vector2.zero);

        return Vector2.zero;
    }
    public virtual void RotateView()
    {
        root.rotation = Quaternion.LookRotation(GetFocus());
    }
    public virtual void NextAction() { }

    //animation functions
    public virtual void Clash() { }
    public virtual void SetAlive(bool _alive) { }
    public virtual bool GetAlive()
    {
        return _alive;
    }
    public virtual void SetGrounded(bool _grounded) { }
    public virtual bool GetGrounded()
    {
        return false;
    }
    public virtual void SetCapped(bool _capped) { }
    public virtual bool GetCapped()
    {
        return false;
    }
    public virtual void SetWallDirection(Vector2 _wallDirection) { }
    public virtual Vector2 GetWallDirection()
    {
        return Vector2.zero;
    }
    public virtual void SetGrab(bool _grab) { }
    public virtual bool GetGrab()
    {
        return false;
    }
    public virtual void SetGrabbed(bool _grabbed) { }
    public virtual bool GetGrabbed()
    {
        return false;
    }
    public virtual void SetSpeed(Vector2 speed) { }
    public virtual Vector2 GetSpeed()
    {
        return new Vector2(rigidbody.velocity.x, rigidbody.velocity.z);
    }
    public virtual void SetHealth(float _health) { }
    public virtual float GetHealth()
    {
        return _health;
    }
    public virtual void SetStun(float _stun) { }
    public virtual float GetStun()
    {
        return _stun;
    }

    //blocking mask
    [System.Serializable]
    [System.Flags]
    public enum BlockingMask
    {
        Rigidbody = (int)0x0001,
        Gravity = (int)0x0002,
        Collision = (int)0x0004,

        Movement = (int)0x0008,
        Orientation = (int)0x0010,
        Action = (int)0x0020,

        GroundStick = (int)0x0040,
        CeilingStick = (int)0x0080,
        WallStick = (int)0x0100,
        Drag = (int)0x0200
    }
    [HideInInspector]
    [EnumFlag("Blocking Mask")]
    public BlockingMask blockingMask;
    [EnumFlag("Fixed Blocking Mask")]
    public BlockingMask fixedBlockingMask;
    private BlockingMask defaultBlockingMask;

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
    private bool m_PreviouslyGrounded, m_PreviouslyCapped;
    private Vector3 m_PreviousWallDirection;
        private float wallCheckSmoothing = 0.2f;
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
    private void CollisionChecks()
    {
        //record previous ground state
        m_PreviouslyGrounded = GetGrounded();
        m_PreviouslyCapped = GetCapped();
        m_PreviousWallDirection = GetWallDirection();

        //variables
        Vector3 extents = collider.bounds.extents;
        float height = extents.y;
        extents = new Vector3(extents.x, 0.0f, extents.z);
        RaycastHit hitInfo;

        //floor check
        if (Physics.SphereCast(transform.position, extents.magnitude, Vector3.down, out hitInfo,
                               (height - extents.magnitude) + 
                               advancedSettings.groundCheckDistance))
        {
            SetGrounded(true);
            m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            SetGrounded(false);
            m_GroundContactNormal = Vector3.up;
        }

        //ceiling check
        if (Physics.SphereCast(transform.position, extents.magnitude, Vector3.up, out hitInfo,
                               (height - extents.magnitude) +
                               advancedSettings.groundCheckDistance))
        {
            SetCapped(true);
        }
        else
        {
            SetCapped(false);
        }

        //wall check
        //iterate clockwise through cardinal directions, find the first collision
        Vector3[] startDirs = { GetFocus(), Quaternion.Euler(0f, 90f, 0f) * GetFocus(), Quaternion.Euler(0f, 180f, 0f) * GetFocus(), Quaternion.Euler(0f, 270f, 0f) * GetFocus() };
        Vector3 testDir = startDirs[0];
        Vector2 wallDir0 = Vector2.zero;
        for (int i = 0; i < startDirs.Length; i++)
        {
            //find start direction
            Vector3 startDir = startDirs[i];

            //find target direction
            Vector3 targetDir = i < startDirs.Length - 1 ? startDirs[i + 1] : startDirs[0];

            //step through all wall checks allowed by smoothing between start and target 
            float _wallCheckSmoothing = 0.0f;
            while (_wallCheckSmoothing < 1.0f)
            {
                //new test direction is the current step between start and target
                testDir = Vector3.Lerp(startDir, targetDir, _wallCheckSmoothing);

                //wall check at test direction
                if (Physics.SphereCast(transform.position, extents.magnitude, testDir, out hitInfo,
                                       (height - extents.magnitude) + advancedSettings.groundCheckDistance))
                    wallDir0 = new Vector2(testDir.x, testDir.z).normalized;
                //cast backwards for safety, exclude collisions with self
                else if(Physics.SphereCast(transform.position + (testDir * ((height - extents.magnitude) + advancedSettings.groundCheckDistance)), extents.magnitude, -1 * testDir, out hitInfo,
                                       (height - extents.magnitude) + advancedSettings.groundCheckDistance) &&
                                       hitInfo.collider.attachedRigidbody != rigidbody)
                    wallDir0 = new Vector2(testDir.x, testDir.z).normalized;

                //next wall smoothing step
                _wallCheckSmoothing += wallCheckSmoothing;
            }
        }
        //second pass, find the first counter-clockwise wall collision
        startDirs = new Vector3[] { GetFocus(), Quaternion.Euler(0f, 270f, 0f) * GetFocus(), Quaternion.Euler(0f, 180f, 0f) * GetFocus(), Quaternion.Euler(0f, 90f, 0f) * GetFocus() };
        testDir = startDirs[0];
        Vector2 wallDir1 = wallDir0;
        for (int i = 0; i < startDirs.Length; i++)
        {
            //find start direction
            Vector3 startDir = startDirs[i];

            //find target direction
            Vector3 targetDir = i < startDirs.Length - 1 ? startDirs[i + 1] : startDirs[0];

            //step through all wall checks allowed by smoothing between start and target 
            float _wallCheckSmoothing = 0.0f;
            while (_wallCheckSmoothing < 1.0f)
            {
                //new test direction is the current step between start and target
                testDir = Vector3.Lerp(startDir, targetDir, _wallCheckSmoothing);

                //wall check at test direction
                if (Physics.SphereCast(transform.position, extents.magnitude, testDir, out hitInfo,
                                       (height - extents.magnitude) + advancedSettings.groundCheckDistance))
                    wallDir1 = new Vector2(testDir.x, testDir.z).normalized;
                //cast backwards for safety, exclude collisions with self
                else if (Physics.SphereCast(transform.position + (testDir * ((height - extents.magnitude) + advancedSettings.groundCheckDistance)), extents.magnitude, -1 * testDir, out hitInfo,
                                       (height - extents.magnitude) + advancedSettings.groundCheckDistance) &&
                                       hitInfo.collider.attachedRigidbody != rigidbody)
                    wallDir1 = new Vector2(testDir.x, testDir.z).normalized;

                //next wall smoothing step
                _wallCheckSmoothing += wallCheckSmoothing;
            }
        }

        //take the average of both passes, convert to local direction using focus direction, and set
        SetWallDirection(Quaternion.LookRotation(GetFocus()) * Vector2.Lerp(wallDir0, wallDir1, 0.5f).normalized);
    }

    //grab functions
    private bool isGrabbing = false;
    public void StartGrabbing(MovingObject other, Vector3 grabOffset)
    {
        if (!isGrabbing)
            StartCoroutine(Grab(other, grabOffset));
    }
    public void StopGrabbing()
    {
        if (isGrabbing)
            stopGrabbing = true;
    }
    private bool stopGrabbing = false;
    public IEnumerator Grab(MovingObject other, Vector3 grabOffset)
    {
        isGrabbing = true;

        SetGrab(true);
        other.SetGrabbed(true);

        stopGrabbing = false;
        while (!stopGrabbing)
        {
            Vector3 position = (Quaternion.LookRotation(GetFocus()) * grabOffset);
            position += root.position;
            other.rigidbody.MovePosition(position);
            Vector3 faceDir = (root.position - other.rigidbody.position).normalized;
            other.rigidbody.MoveRotation(Quaternion.LookRotation(faceDir));

            yield return null;
        }
        stopGrabbing = false;

        SetGrab(false);
        other.SetGrabbed(false);

        isGrabbing = false;
    }

    //character stats
    public virtual void Dead()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = defaultBlockingMask;
        ResetStun();

        gameObject.SetActive(false);
    }
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

            SetAlive(_alive);
		}
	}
    public virtual void Kill ()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = defaultBlockingMask;
        ResetStun();

        alive = false;
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

            SetHealth(_health);

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

            SetStun(_stun);
		}
	}
	public float stunRecovery;
	public virtual void ResetStun ()
    {
		stun = 0.0f;
	}
    private Vector2 input = Vector2.zero;

    public Transform root;

    //monobehaviour functions
    public virtual void Awake()
    {
        defaultBlockingMask = blockingMask;

        if (rigidbody.isKinematic)
            defaultBlockingMask |= BlockingMask.Rigidbody;
        if (!rigidbody.useGravity)
            defaultBlockingMask |= BlockingMask.Gravity;
        if (collider.isTrigger)
            defaultBlockingMask |= BlockingMask.Collision;

        if (root == null)
            root = transform;
    }
	public virtual void Start ()
    {
        isGrabbing = false;
        stopGrabbing = false;
        blockingMask = defaultBlockingMask;
        alive = true;
        ResetHealth();
        ResetStun();
    }
	public virtual void Update ()
    {
        NextAction();
        RotateView();
        input = GetInput();

        if (GetGrounded())
            StopGrabbing();

        if (health < maxHealth)
            health += healthRegen * Time.deltaTime;
        if (stunned)
            stun -= stunRecovery * Time.deltaTime;
    }
    public virtual void FixedUpdate()
    {
        blockingMask = (fixedBlockingMask & defaultBlockingMask) | (~fixedBlockingMask & blockingMask);

        rigidbody.isKinematic = (blockingMask & BlockingMask.Rigidbody) != 0;
        rigidbody.useGravity = (blockingMask & BlockingMask.Gravity) == 0;
        collider.isTrigger = (blockingMask & BlockingMask.Collision) != 0;

        CollisionChecks();

        if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || GetGrounded()))
        {
            Vector3 desiredMove = GetFocus() * input.y + (Quaternion.Euler(0.0f, 90.0f, 0.0f) * GetFocus()) * input.x;
            desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

            desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
            desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
            desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
            if (rigidbody.velocity.sqrMagnitude <
                (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
            {
                rigidbody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.VelocityChange);
            }
        }

        if((advancedSettings.airControl || GetGrounded() || GetCapped() || GetWallDirection() != Vector2.zero) && (blockingMask & BlockingMask.Drag) == 0)
        {
            rigidbody.drag = 5f;
        }
        else
        {
            rigidbody.drag = 0f;
        }

        if (!GetGrounded() && m_PreviouslyGrounded && (blockingMask & BlockingMask.GroundStick) == 0)
        {
            StickToGroundHelper();
        }
        if (!GetCapped() && m_PreviouslyCapped && (blockingMask & BlockingMask.CeilingStick) == 0)
        {

        }
        if (GetWallDirection() == Vector2.zero && m_PreviousWallDirection != Vector3.zero && (blockingMask & BlockingMask.WallStick) == 0)
        {

        }
    }
}