using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BareKnuckle : MoveSet
{
    public void ToggleGrab(bool _grab)
    {
        dualAnimator.SetBool("Grab", _grab);
        leftAnimator.SetBool("Grab", _grab);
        rightAnimator.SetBool("Grab", _grab);
    }
    public void ToggleWalled(Vector3 _walled)
    {
        dualAnimator.SetBool("Walled", _walled.z > 0.5f);
        leftAnimator.SetBool("Walled", _walled.x > 0.1f && _walled.z <= 0.5f);
        rightAnimator.SetBool("Walled", _walled.x < -0.1f && _walled.z <= 0.5f);
    }
}
