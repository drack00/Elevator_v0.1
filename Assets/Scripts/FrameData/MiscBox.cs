using UnityEngine;
using System.Collections;

public class MiscBox : ActiveFrameData
{
    [EnumFlag("Blocking Mask")]
    public MovingObject.BlockingMask blockingMask;

    public Behaviour enableBehaviour;
	public Behaviour disableBehaviour;
	public Behaviour updateBehaviour;

    public void OnEnable()
    {
        mo.blockingMask |= blockingMask;

        enableBehaviour.Do(false, this);
    }
    public void OnDisable()
    {
        mo.blockingMask = mo.blockingMask & (mo.blockingMask ^ blockingMask);
        
        disableBehaviour.Do(false, this);
    }
}