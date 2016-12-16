using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AI_Master))]
public class AIControlled_MovingObject : MovingObject
{
    public AI_Master ai
    {
        get
        {
            return GetComponent<AI_Master>();
        }
    }

    public virtual void Awake ()
    {
        ai.movement.agent.updatePosition = false;
        ai.movement.agent.updateRotation = false;
        ai.movement.agent.Stop();
    }

    public float acceleration;
    public float maxSpeed;

    public virtual void FixedUpdate()
    {
        GroundCheck();
        Vector3 navDirection = ai.movement.agent.destination - transform.position;

        if ((navDirection.sqrMagnitude > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
        {
            Vector3 desiredForce = navDirection.normalized * acceleration;
            Vector3 sumForce = rigidbody.velocity + desiredForce;
            Vector3 force = Vector3.ClampMagnitude(sumForce, maxSpeed) - rigidbody.velocity;
            rigidbody.AddForce(force, ForceMode.Acceleration);
        }
    }

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
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
    private void GroundCheck()
    {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo, ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance))
            m_IsGrounded = true;
        else
            m_IsGrounded = false;
        if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            m_Jumping = false;
    }
}
 
 