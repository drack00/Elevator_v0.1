using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FixedJoint))]
public class GrabBox : HurtBox
{
    public override bool Clash(HitBox hit, bool continuous)
    {
        if (hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
            hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>().Grab();

        if (collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
            collider.attachedRigidbody.gameObject.GetComponent<MovingObject>().Grabbed();

        return true;
    }

    public void Release(HitBox hit)
    {
        if (hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
            hit.collider.attachedRigidbody.gameObject.GetComponent<MovingObject>().Release();

        if (collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
            collider.attachedRigidbody.gameObject.GetComponent<MovingObject>().Released();
    }
}
