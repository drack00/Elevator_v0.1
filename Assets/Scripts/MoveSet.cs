using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSet : MonoBehaviour
{
    public static MoveSet[] allMoveSets
    {
        get
        {
            return FindObjectsOfType<MoveSet>();
        }
    }
    public static MoveSet GetMoveSet(Animator animator)
    {
        foreach(MoveSet moveSet in allMoveSets)
        {
            if (moveSet.dualAnimator == animator || moveSet.leftAnimator == animator || moveSet.rightAnimator == animator)
                return moveSet;
        }

        return null;
    }
    public MoveSet[] allOtherMoveSets
    {
        get
        {
            List<MoveSet> _allOtherMoveSets = new List<MoveSet>(transform.parent.GetComponentsInChildren<MoveSet>());

            _allOtherMoveSets.Remove(this);

            return _allOtherMoveSets.ToArray();
        }
    }

    [System.Serializable]
    [System.Flags]
    public enum ActiveInputs
    {
        Left = 0x0001, Right = 0x0002, Dual = Left | Right
    }
    public ActiveInputs activeInputs
    {
        get
        {
            if (!dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))
                return ActiveInputs.Dual;
            if (!leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))
                return ActiveInputs.Left;
            if (!rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Rest"))
                return ActiveInputs.Right;

            return 0;
        }
    }
    public ActiveInputs canCancelInputs
    {
        get
        {
            if (dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCancel") || dualAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge"))
                return ActiveInputs.Dual;
            if (leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCancel") || leftAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge"))
                return ActiveInputs.Left;
            if (rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCancel") || rightAnimator.GetCurrentAnimatorStateInfo(0).IsTag("CanCharge"))
                return ActiveInputs.Right;

            return 0;
        }
    }

    public Animator dualAnimator;
    public Animator leftAnimator;
    public Animator rightAnimator;
    public float xRotation;

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

    public void Reset()
    {
        dualAnimator.ResetTrigger("Positive");
        dualAnimator.ResetTrigger("Negative");
        dualAnimator.SetBool("Hold", false);

        leftAnimator.ResetTrigger("Positive");
        leftAnimator.ResetTrigger("Negative");
        leftAnimator.SetBool("Hold", false);

        rightAnimator.ResetTrigger("Positive");
        rightAnimator.ResetTrigger("Negative");
        rightAnimator.SetBool("Hold", false);
    }
}