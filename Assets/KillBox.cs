using UnityEngine;
using System.Collections;

public class KillBox : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<MovingObject>() == null)
            return;

        other.GetComponent<MovingObject>().Kill();
    }
}
