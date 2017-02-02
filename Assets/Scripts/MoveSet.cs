using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSet : MonoBehaviour
{
    [HideInInspector]
    public Animator mainAnimator;
    public Animator leftAnimator;
    public Animator rightAnimator;
    [HideInInspector]
    public UIGizmo[] gizmo;
    public float xRotation;

    void Awake()
    {
        mainAnimator = GetComponent<Animator>();
        gizmo = GetComponentsInChildren<UIGizmo>();
    }

    public void Reset()
    {
        mainAnimator.ResetTrigger("Positive");
        leftAnimator.ResetTrigger("Positive");
        rightAnimator.ResetTrigger("Positive");

        mainAnimator.ResetTrigger("Negative");
        leftAnimator.ResetTrigger("Negative");
        rightAnimator.ResetTrigger("Negative");
    }

    public void ToggleActive(bool _active)
    {
        mainAnimator.SetBool("Active", _active);
        leftAnimator.SetBool("Active", _active);
        rightAnimator.SetBool("Active", _active);
    }
    public void ToggleGrounded(bool _grounded)
    {
        mainAnimator.SetBool("Grounded", _grounded);
        leftAnimator.SetBool("Grounded", _grounded);
        rightAnimator.SetBool("Grounded", _grounded);
    }
    public void ToggleHoldInput(bool left, bool right)
    {
        mainAnimator.SetBool("Hold", left && right);
        leftAnimator.SetBool("Hold", left);
        rightAnimator.SetBool("Hold", right);
    }

    public void SetPositiveInput(bool left, bool right)
    {
        if (left && right)
        {
            mainAnimator.SetTrigger("Positive");

            return;
        }

        if (left)
        {
            leftAnimator.SetTrigger("Positive");

            return;
        }

        if (right)
        {
            rightAnimator.SetTrigger("Positive");
        }
    }
    public void SetNegativeInput(bool left, bool right)
    {
        if (left && right)
        {
            mainAnimator.SetTrigger("Negative");

            return;
        }

        if (left)
        {
            leftAnimator.SetTrigger("Negative");

            return;
        }

        if (right)
        {
            rightAnimator.SetTrigger("Negative");
        }
    }
}