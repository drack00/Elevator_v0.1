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
    public bool restrictRotation;
    public Transform root;

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

    public override void Update()
    {
        base.Update();

        if ((blockingMask & BlockingMask.AI_Orientation) == 0 && (m_IsGrounded || airControl))
        {
            Quaternion rotation = ai.orientation.desiredRotation;
            if (restrictRotation)
                rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
            root.rotation = rotation;
        }

        animator.SetBool("Grounded", m_IsGrounded);
        animator.SetFloat("MoveSpeed", rigidbody.velocity.magnitude);

        if (m_IsGrounded)
            StopGrabbing();
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
 
 