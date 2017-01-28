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
    public override void SetCapped(bool _capped)
    {
        animator.SetBool("Capped", _capped);
    }
    public override bool GetCapped()
    {
        return animator.GetBool("Capped");
    }
    public override void SetWallDirection(Vector3 _wallDirection)
    {
        base.SetWallDirection(_wallDirection);

        animator.SetFloat("Walled_X", _wallDirection.x);
        animator.SetFloat("Walled_Y", _wallDirection.y);
        animator.SetFloat("Walled_Z", _wallDirection.z);
    }
    public override Vector3 GetWallDirection()
    {
        return new Vector3(animator.GetFloat("Walled_X"), animator.GetFloat("Walled_Y"), animator.GetFloat("Walled_Z"));
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
    public override void SetSpeed(Vector2 speed)
    {
        animator.SetFloat("Speed_Y", speed.y);
        animator.SetFloat("Speed_X", speed.x);
    }
    public override Vector2 GetSpeed()
    {
        return new Vector2(animator.GetFloat("Speed_X"), animator.GetFloat("Speed_Y"));
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
