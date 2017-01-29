﻿using UnityEngine;
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

        Vector2 input = new Vector2
        {
            x = ai.movement.agent.velocity.x,
            y = ai.movement.agent.velocity.z
        };
        input.Normalize();

        SetSpeed(input);

        movementSettings.UpdateDesiredTargetSpeed(Quaternion.Inverse(Quaternion.LookRotation(GetFocus())) * input);

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
    public override void Update()
    {
        base.Update();
    }
}