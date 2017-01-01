using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Action : MonoBehaviour
{
    [System.Serializable]
    public enum Subroutine
    {
        Masked, None, Attack
    }

    public interface ISubroutine
    {
        void Do(AI_Action action);
    }

    public Animator animator;

    [System.Serializable]
    public class Default : ISubroutine
    {
        public void Do (AI_Action action)
        {

        }
    }
    public Default defaultSubroutine;

    [System.Serializable]
    public class Attack : ISubroutine
    {
        public void Do(AI_Action action)
        {
            action.animator.SetTrigger("Attack");
        }
    }
    public Attack attackSubroutine;

    [HideInInspector]
    public Subroutine selectedSubroutine;
    [HideInInspector]
    public Transform activeTarget;

    public void Awake()
    {
        selectedSubroutine = Subroutine.None;
    }

    [System.Serializable]
    public enum UpdateOn
    {
        None, FixedUpdate, Update, LateUpdate
    }
    public UpdateOn updateOn;
    public void FixedUpdate()
    {
        if (updateOn == UpdateOn.FixedUpdate)
            DoUpdate(Time.fixedDeltaTime);
    }
    public void Update()
    {
        if (updateOn == UpdateOn.Update)
            DoUpdate(Time.deltaTime);
    }
    public void LateUpdate()
    {
        if (updateOn == UpdateOn.LateUpdate)
            DoUpdate(Time.deltaTime);
    }
    private void DoUpdate(float timeDelta)
    {
        ISubroutine activeSubroutine = defaultSubroutine;

        switch (selectedSubroutine)
        {
            case Subroutine.Attack:
                activeSubroutine = attackSubroutine;
                break;
        }

        activeSubroutine.Do(this);
    }
}
