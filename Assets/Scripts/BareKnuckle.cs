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
    public void ToggleWalled(Vector2 _walled)
    {
        dualAnimator.SetBool("Walled", _walled.y > 0.5f);
        leftAnimator.SetBool("Walled", _walled.x > 0.1f);
        rightAnimator.SetBool("Walled", _walled.x < -0.1f);
    }
}
