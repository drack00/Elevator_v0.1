using UnityEngine;
using System.Collections;

public class MiscBox : ActiveFrameData
{
    public bool additive = true;
    [EnumFlag("Blocking Mask")]
    public MovingObject.BlockingMask blockingMask;

    public Behaviour enableBehaviour;
	public Behaviour disableBehaviour;
	public Behaviour updateBehaviour;

    public void OnEnable()
    {
        enableBehaviour.Do(false, this);

        if (additive && collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
        {
            MovingObject mo = collider.attachedRigidbody.gameObject.GetComponent<MovingObject>();

            mo.blockingMask |= blockingMask;
        }
    }
    public void OnDisable()
    {
        if (additive && collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
        {
            MovingObject mo = collider.attachedRigidbody.gameObject.GetComponent<MovingObject>();

            mo.blockingMask ^= blockingMask;
        }

        disableBehaviour.Do(false, this);
    }
    public void Update ()
    {
        if (!additive && collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
        {
            MovingObject mo = collider.attachedRigidbody.gameObject.GetComponent<MovingObject>();

            mo.blockingMask = blockingMask;
        }

        updateBehaviour.Do(true, this);
    }
}