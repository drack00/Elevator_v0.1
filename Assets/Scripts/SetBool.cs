using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetBool : MonoBehaviour
{
    public Animator animator;
    public string boolName;
    void OnEnable()
    {
        animator.SetBool(boolName, true);
    }
    void OnDisable()
    {
        animator.SetBool(boolName, false);
    }
}
