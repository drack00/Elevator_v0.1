using UnityEngine;
using System.Collections;

public class MiscBox : ActiveFrameData
{
    [System.Serializable]
    public enum Phase
    {
        Enable, Disable, Update
    }

    [System.Flags]
    [System.Serializable]
    public enum BlockingMask
    {
        LeftInputs = 0x0001,
        RightInputs = 0x0002,
        Controller = 0x0004,
        Rigidbody = 0x0008
    }

    public Behaviour enableBehaviour;
	public Behaviour disableBehaviour;
	public Behaviour updateBehaviour;

    public bool additive = true;
    [EnumFlag("Blocking Mask")]
    public BlockingMask blockingMask;

    public void OnEnable ()
    {
        if (additive && collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
        {
            MovingObject mo = collider.attachedRigidbody.gameObject.GetComponent<MovingObject>();

            mo.blockingMask += (int)blockingMask;
        }

        enableBehaviour.Do(false, this);
	}
    public void OnDisable()
    {
        if (additive && collider.attachedRigidbody.gameObject.GetComponent<MovingObject>() != null)
        {
            MovingObject mo = collider.attachedRigidbody.gameObject.GetComponent<MovingObject>();

            mo.blockingMask -= (int)blockingMask;
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