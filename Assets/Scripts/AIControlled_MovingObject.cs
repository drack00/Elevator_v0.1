using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AI))]
public class AIControlled_MovingObject : MovingObject
{
    public AI ai
    {
        get
        {
            return GetComponent<AI>();
        }
    }

    public virtual void Awake ()
    {
        ai.OnAwake();
    }
    public float maxSpeed;
    public virtual void FixedUpdate()
    {
        ai.OnUpdate();

        GroundCheck();
        Vector3 navDirection = (ai.agent.destination - transform.position).normalized;
        //movementSettings.UpdateDesiredTargetSpeed(navDirection);

        if ((navDirection.sqrMagnitude > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
        {
            float desiredDir = MathStuff.SignedAngleBetween(transform.forward, navDirection, Vector3.up) * Mathf.Rad2Deg;
            rigidbody.AddTorque(new Vector3(0.0f, (-1 * rigidbody.angularVelocity.y) + desiredDir, 0.0f), ForceMode.Impulse);

            Vector3 desiredMove = transform.forward * maxSpeed;
            rigidbody.AddForce((-1 * rigidbody.velocity) + desiredMove, ForceMode.Impulse);
        }
    }

    /*[System.Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 8.0f;
        public float BackwardSpeed = 4.0f;
        public float StrafeSpeed = 4.0f;
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector]public float CurrentTargetSpeed = 8f;

        public void UpdateDesiredTargetSpeed(Vector3 input)
        {
            if (input == Vector3.zero) return;
            if (input.x > 0 || input.x < 0)
                CurrentTargetSpeed = StrafeSpeed;
            if (input.y < 0)
                CurrentTargetSpeed = BackwardSpeed;
            if (input.y > 0)
                CurrentTargetSpeed = ForwardSpeed;
        }
    }
    public MovementSettings movementSettings = new MovementSettings();*/
    [System.Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f;
        public float stickToGroundHelperDistance = 0.5f;
        public float slowDownRate = 20f;
        public bool airControl;
    }
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    private CapsuleCollider m_Capsule
    {
        get
        {
            return GetComponent<CapsuleCollider>();
        }
    }
    //private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
    private void GroundCheck()
    {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo, ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance))
        {
            m_IsGrounded = true;
            //m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            m_IsGrounded = false;
            //m_GroundContactNormal = Vector3.up;
        }
        if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            m_Jumping = false;
    }
    /*private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }*/
}
 
 