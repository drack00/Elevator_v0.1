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

    public bool airControl;

    public virtual void Awake ()
    {
        ai.movement.agent.updatePosition = false;
        ai.movement.agent.updateRotation = false;
        ai.movement.agent.Stop();
    }

    public override void Start()
    {
        base.Start();

        ai.movement.agent.Resume();
    }

    public virtual void FixedUpdate()
    {
        GroundCheck();

        if ((blockingMask & BlockingMask.AI_Movement) == 0 && (m_IsGrounded || airControl))
        {
            ai.movement.agent.nextPosition = rigidbody.position;

            Vector3 velocityChange = Vector3.ClampMagnitude(ai.movement.agent.velocity - rigidbody.velocity, ai.movement.agent.speed);
            rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
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
 
 