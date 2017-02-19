using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private bool _unload = false;
    public bool unload
    {
        get
        {
            return _unload;
        }
        set
        {
            unloading = true;

            foreach (Transform piece in transform)
            {
                piece.GetComponent<Animator>().SetTrigger("Unload");
            }

            _unload = value;
        }
    }
    [HideInInspector]
    public bool unloading = false;
    [HideInInspector]
    public bool stopToUnload = true;
    
    void FixedUpdate()
    {
        if (unloading)
        {
            foreach (Transform piece in transform)
            {
                if (piece.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Unloading"))
                    return;
            }

            unloading = false;
        }
    }
}
