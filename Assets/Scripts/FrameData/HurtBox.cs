using UnityEngine;
using System.Collections;

public class HurtBox : FrameData
{
	[System.Serializable]
	public class Multiplier
    {
		private float multiplier
        {
			get
            {
				return falloff.Evaluate (angle);
			}
		}
        public AnimationCurve falloff = AnimationCurve.Linear(1.0f, 1.0f, 1.0f, 1.0f);

		public float GetAngle (float timeDelta = 1.0f)
        {
			float _multiplier = multiplier;

			AngleUp (timeDelta);

			return _multiplier;
		}

		private float angle;
		public float angleUpSpeed;
		public float angleDownSpeed;

		public void AngleUp (float deltaTime = 1.0f)
        {
			angle += angleUpSpeed * deltaTime;
		}
		public void AngleDown (float deltaTime = 1.0f)
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

	public Multiplier force;
    public Multiplier torque;

    public bool willClash;
    public bool continuousClash;
    public bool overrideHit;

	public void Start ()
    {
        force.Reset ();
        torque.Reset ();
	}

	public void FixedUpdate ()
    {
        force.AngleDown (Time.fixedDeltaTime);
        torque.AngleDown (Time.fixedDeltaTime);
	}
}