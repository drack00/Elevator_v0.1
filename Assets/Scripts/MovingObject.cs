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

    [System.Serializable]
    [System.Flags]
    public enum BlockingMask
    {
        LeftInputs = 1,
        RightInputs = 2,
        Controller = 4,
        Rigidbody = 8
    }

    //[HideInInspector]
    [EnumFlag("Blocking Mask")]
    public BlockingMask blockingMask;

    public virtual void Clash (){}
		
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
        blockingMask = 0;
        ResetStun();

        alive = false;
    }
    public virtual void Dead ()
    {
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