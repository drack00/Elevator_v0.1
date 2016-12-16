using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Action : MonoBehaviour {
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

    public bool fixedUpdate;
    public void FixedUpdate()
    {
        if (!fixedUpdate)
            return;

        OnUpdate();
    }
    public void Update()
    {
        if (fixedUpdate)
            return;

        OnUpdate();
    }
    private void OnUpdate()
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
