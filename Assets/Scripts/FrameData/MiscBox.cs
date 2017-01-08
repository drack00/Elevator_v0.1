using UnityEngine;
using System.Collections;

public class MiscBox : ActiveFrameData
{
    public MovingObject.BlockingMask blockingMask;
    public bool additive = true;

    public Behaviour enableBehaviour;
	public Behaviour disableBehaviour;
	public Behaviour updateBehaviour;

    public void OnEnable()
    {
        enableBehaviour.Do(false, this);

        if (additive)
            mo.blockingMask |= blockingMask;
    }
    public void OnDisable()
    {
        if (additive)
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