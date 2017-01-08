using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(Rigidbody))]
public class MovingObject : MonoBehaviour
{
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

    //blocking mask
    [System.Serializable]
    [System.Flags]
    public enum BlockingMask
    {
        Physics = Rigidbody | Gravity,
        Rigidbody = 1,
        Gravity = 2,

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

	public virtual void Start ()
    {
        Spawn ();
	}

	public virtual void Update ()
    {
		if (health < maxHealth)
			health += healthRegen * Time.deltaTime;
		if (stunned)
			stun -= stunRecovery * Time.deltaTime;
	}
}