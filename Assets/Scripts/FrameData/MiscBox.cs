using UnityEngine;
using System.Collections;

public class MiscBox : ActiveFrameData
{
    [EnumFlag("Blocking Mask")]
    public MovingObject.BlockingMask blockingMask;
    public bool additive = true;

    public Behaviour enableBehaviour;
	public Behaviour disableBehaviour;
	public Behaviour updateBehaviour;

    public void OnEnable()
    {
        if (additive)
            mo.blockingMask |= blockingMask;

        enableBehaviour.Do(false, this);
    }
    public void OnDisable()
    {
        mo.blockingMask ^= blockingMask;
        
        disableBehaviour.Do(false, this);
    }
    public void Update ()
    {
        if (!additive)
            mo.blockingMask = blockingMask;

        updateBehaviour.Do(true, this);
    }
}