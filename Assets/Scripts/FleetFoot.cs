using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetFoot : MoveSet
{
    public void ToggleMovement(Vector2 _walled)
    {
        mainAnimator.SetBool("XMovement", _walled.x > 0.1f || _walled.x < -0.1f);
        leftAnimator.SetBool("XMovement", _walled.x > 0.1f || _walled.x < -0.1f);
        rightAnimator.SetBool("XMovement", _walled.x > 0.1f || _walled.x < -0.1f);

        mainAnimator.SetBool("YMovement", _walled.y > 0.1f || _walled.y < -0.1f);
        leftAnimator.SetBool("YMovement", _walled.y > 0.1f || _walled.y < -0.1f);
        rightAnimator.SetBool("YMovement", _walled.y > 0.1f || _walled.y < -0.1f);
    }
}
