using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AI_Master))]
public class Enemy : AnimatedMovingObject
{
    public AI_Master ai
    {
        get
        {
            return GetComponent<AI_Master>();
        }
    }

    public override Vector3 GetFocus()
    {
        return new Vector3(root.forward.x, 0f, root.forward.z).normalized;
    }
    public override Vector2 GetInput()
    {
        if ((blockingMask & BlockingMask.Movement) != 0)
            return base.GetInput();

        Vector3 worldDir = new Vector3(ai.movement.agent.velocity.x, 0f, ai.movement.agent.velocity.z).normalized;
        Quaternion inverseRot = Quaternion.Inverse(Quaternion.LookRotation(GetFocus()));
        Vector3 localDir = (inverseRot * worldDir).normalized;
        Vector2 input = new Vector2(localDir.x, localDir.z).normalized;

        SetSpeed(input);

        movementSettings.UpdateDesiredTargetSpeed(input);

        return input;
    }
    public override void RotateView()
    {
        if ((blockingMask & BlockingMask.Orientation) != 0)
            return;

        Quaternion desiredRotation = Quaternion.Euler(0.0f, ai.orientation.desiredRotation.eulerAngles.y, 0.0f);
        root.rotation = desiredRotation;
    }
    public override void NextAction()
    {
        if ((blockingMask & BlockingMask.Action) != 0 || string.IsNullOrEmpty(ai.action.action))
            return;

        animator.SetTrigger(ai.action.action);
    }

    public override void Awake()
    {
        base.Awake();

        ai.movement.agent.updatePosition = false;
        ai.movement.agent.updateRotation = false;
        ai.movement.agent.Stop();
    }
    public override void Start()
    {
        base.Start();

        ai.movement.agent.Resume();
    }

    private Vector3 gizmo = Vector3.zero;
    void OnDrawGizmos()
    {
        if (gizmo != Vector3.zero)
            Gizmos.DrawRay(transform.position, gizmo);
    }
}