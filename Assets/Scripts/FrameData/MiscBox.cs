using UnityEngine;
using System.Collections;

public class MiscBox : ActiveFrameData
{
    public Behaviour enableBehaviour;
	public Behaviour disableBehaviour;
	public Behaviour updateBehaviour;

    public void OnEnable()
    {
        if (collider == null ||
            collider.attachedRigidbody == null)
            return;

        enableBehaviour.Do(false, this);
    }
    public void OnDisable()
    {
        if (collider == null ||
            collider.attachedRigidbody == null)
            return;
        
        disableBehaviour.Do(false, this);
    }
    public void Update ()
    {
        if (collider == null ||
            collider.attachedRigidbody == null)
            return;

        updateBehaviour.Do(true, this);
    }
}