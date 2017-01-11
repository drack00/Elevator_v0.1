using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AI_Master))]
public class Enemy : MovingObject
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

    public override void Awake()
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

        if ((blockingMask & BlockingMask.AI_Orientation) == 0 && (Grounded || airControl))
        {
            Quaternion rotation = ai.orientation.desiredRotation;
            if (restrictRotation)
                rotation = Quaternion.Euler(0.0f, rotation.eulerAngles.y, 0.0f);
            root.rotation = rotation;
        }

        animator.SetBool("Grounded", Grounded);
        Vector3 velocity = rigidbody.velocity;
        velocity = new Vector3(velocity.x, 0.0f, velocity.z);
        animator.SetFloat("MoveSpeed", velocity.magnitude);

        if (Grounded)
            StopGrabbing();

        if ((blockingMask & BlockingMask.AI_Action) == 0 && !string.IsNullOrEmpty(ai.action.action))
            animator.SetTrigger(ai.action.action);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if ((blockingMask & BlockingMask.AI_Movement) == 0 && (Grounded || airControl))
        {
            ai.movement.agent.nextPosition = rigidbody.position;

            Vector3 velocityChange = Vector3.ClampMagnitude(ai.movement.agent.velocity - rigidbody.velocity, ai.movement.agent.speed);
            rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}