using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetInputs : MonoBehaviour
{
    public Animator animator;

    void OnEnable()
    {
        animator.ResetTrigger("Positive");
    }
    void Update()
    {
        animator.ResetTrigger("Negative");
    }
}
