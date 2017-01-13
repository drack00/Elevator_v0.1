using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Animator))]
public class AnimatedMovingObject : MovingObject
{
    public Animator animator
    {
        get
        {
            return GetComponent<Animator>();
        }
    }

    public override void Clash()
    {
        animator.SetTrigger("Clash");
    }
    public override void SetAlive(bool _alive)
    {
        animator.SetBool("Alive", _alive);
    }
    public override bool GetAlive()
    {
        return animator.GetBool("Alive");
    }
    public override void SetGrounded(bool _grounded)
    {
        animator.SetBool("Grounded", _grounded);
    }
    public override bool GetGrounded()
    {
        return animator.GetBool("Grounded");
    }
    public override void SetGrab(bool _grab)
    {
        animator.SetBool("Grab", _grab);
    }
    public override bool GetGrab()
    {
        return animator.GetBool("Grab");
    }
    public override void SetGrabbed(bool _grabbed)
    {
        animator.SetBool("Grabbed", _grabbed);
    }
    public override bool GetGrabbed()
    {
        return animator.GetBool("Grabbed");
    }
    public override void SetMoveSpeed(float _moveSpeed)
    {
        animator.SetFloat("MoveSpeed", _moveSpeed);
    }
    public override float GetMoveSpeed()
    {
        return animator.GetFloat("MoveSpeed");
    }
    public override void SetHealth(float _health)
    {
        animator.SetFloat("Health", _health);
    }
    public override float GetHealth()
    {
        return animator.GetFloat("Health");
    }
    public override void SetStun(float _stun)
    {
        animator.SetFloat("Stun", _stun);
    }
    public override float GetStun()
    {
        return animator.GetFloat("Stun");
    }
}
