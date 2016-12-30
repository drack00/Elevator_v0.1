using UnityEngine;
using System.Collections;

public class Hurt : MonoBehaviour
{
	public new Collider collider
    {
		get
        {
			return GetComponent<Collider> ();
		}
	}

	[System.Serializable]
	public struct MultiplierBehaviour
    {
		private float multiplier
        {
			get
            {
				return multiplierFalloff.Evaluate (angle);
			}
		}
		public AnimationCurve multiplierFalloff;

		public float GetAngleInstant ()
        {
			float _multiplier = multiplier;

			AngleUp ();

			return _multiplier;
		}
		public float GetAngleContinuous (float timeDelta)
        {
			float _multiplier = multiplier;

			AngleUp (timeDelta);

			return _multiplier;
		}

		private float angle;
		public float angleUpSpeed;
		public float angleDownSpeed;

		public void AngleUp ()
        {
			angle += angleUpSpeed;
		}
		public void AngleUp (float deltaTime)
        {
			angle += angleUpSpeed * deltaTime;
		}
		public void AngleDown (float deltaTime)
        {
			if (angle > 0.0f)
				angle -= angleDownSpeed * deltaTime;
			if (angle < 0.0f)
				angle = 0.0f;
		}

		public void Reset ()
        {
			angle = 0.0f;
		}
	}

	public MultiplierBehaviour torqueInstant;
	public MultiplierBehaviour torqueContinuous;
	public MultiplierBehaviour forceInstant;
	public MultiplierBehaviour forceContinuous;

	public bool willClash;
    public bool continuousClash;
    public bool overrideHit;

	void Start ()
    {
		torqueInstant.Reset ();
		torqueContinuous.Reset ();
		forceInstant.Reset ();
		forceContinuous.Reset ();
	}

	void FixedUpdate ()
    {
		torqueInstant.AngleDown (Time.fixedDeltaTime);
		torqueContinuous.AngleDown (Time.fixedDeltaTime);
		forceInstant.AngleDown (Time.fixedDeltaTime);
		forceContinuous.AngleDown (Time.fixedDeltaTime);
	}
}