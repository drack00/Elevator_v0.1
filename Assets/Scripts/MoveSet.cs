using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSet : MonoBehaviour
{
    public Animator dualAnimator;
    public Animator leftAnimator;
    public Animator rightAnimator;
    [HideInInspector]
    public UIGizmo[] gizmo;
    public float xRotation;

    void Awake()
    {
        gizmo = GetComponentsInChildren<UIGizmo>();
    }

    public void ToggleActive(bool _active)
    {
        dualAnimator.SetBool("Active", _active);
        leftAnimator.SetBool("Active", _active);
        rightAnimator.SetBool("Active", _active);
    }
    public void ToggleGrounded(bool _grounded)
    {
        dualAnimator.SetBool("Grounded", _grounded);
        leftAnimator.SetBool("Grounded", _grounded);
        rightAnimator.SetBool("Grounded", _grounded);
    }
    public void ToggleHoldInput(bool left, bool right)
    {
        dualAnimator.SetBool("Hold", left && right);
        leftAnimator.SetBool("Hold", left);
        rightAnimator.SetBool("Hold", right);
    }

    public void SetPositiveInput(bool left, bool right)
    {
        if (left && right &&
            leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") && 
            rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))
            dualAnimator.SetTrigger("Positive");

        else if (left && 
            dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))
            leftAnimator.SetTrigger("Positive");

        else if (right &&
            dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))
            rightAnimator.SetTrigger("Positive");
    }
    public void SetNegativeInput(bool left, bool right)
    {
        if (left && right &&
            leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            dualAnimator.SetTrigger("Negative");

        else if (left && 
            dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            leftAnimator.SetTrigger("Negative");

        else if (right && 
            dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest") &&
            rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Charge"))
            rightAnimator.SetTrigger("Negative");
    }
}