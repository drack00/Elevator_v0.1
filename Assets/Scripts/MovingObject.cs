using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class MovingObject : MonoBehaviour, AI.ICanCheck {
    public float GetHealth() { return health; }
    public float GetStun() { return stun; }
    public Animator GetAnimator() { return animator; }

    public Animator animator;
	[HideInInspector]public new Rigidbody rigidbody {
		get {
			return GetComponent<Rigidbody> ();
		}
	}

	public virtual void Clash (){}
		
	private bool _alive;
	public bool alive {
		get {
			return _alive;
		}
		set {
			_alive = value;

			animator.SetBool ("Alive", _alive);
		}
	}

    public virtual void Dead ()
    {
        gameObject.SetActive(false);
    }

	public float maxHealth;
	private float _health;
	public float health {
		get {
			return _health;
		}
		set {
			if (!alive)
				return;

			if (value > maxHealth)
				value = maxHealth;
			if (value < 0.0f)
				value = 0.0f;

			_health = value;

			animator.SetFloat ("Health", _health);

			if (Mathf.Approximately (_health, Mathf.Epsilon))
				alive = false;
		}
	}
	public float healthRegen;
	public void ResetHealth () {
		health = maxHealth;
	}
		
	public bool stunned {
		get {
			return stun > 0.0f;
		}
	}
	public float maxStun;
	private float _stun;
	public float stun {
		get {
			return _stun;
		}
		set {
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
	public void ResetStun () {
		stun = 0.0f;
	}

	public void Spawn () {
		alive = true;
		ResetHealth ();
		ResetStun ();
	}

	public virtual void Start () {
		Spawn ();
	}

	public virtual void Update () {
		if (health < maxHealth)
			health += healthRegen * Time.deltaTime;
		if (stunned)
			stun -= stunRecovery * Time.deltaTime;
	}
}