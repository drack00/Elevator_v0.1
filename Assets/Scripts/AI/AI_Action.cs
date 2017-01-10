using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Action : MonoBehaviour
{
    [System.Serializable]
    public enum Subroutine
    {
        Masked, None, Attack, Dodge
    }

    public interface ISubroutine
    {
        void Do(AI_Action action);
    }

    public string action = null;

    [System.Serializable]
    public class Default : ISubroutine
    {
        public void Do (AI_Action action)
        {
            action.action = null;
        }
    }
    public Default defaultSubroutine;

    [System.Serializable]
    public class Attack : ISubroutine
    {
        public void Do(AI_Action action)
        {
            action.action = "Attack";
        }
    }
    public Attack attackSubroutine;

    [System.Serializable]
    public class Dodge : ISubroutine
    {
        public void Do(AI_Action action)
        {
            action.action = "Dodge";
        }
    }
    public Dodge dodgeSubroutine;

    [HideInInspector]
    public Subroutine selectedSubroutine;
    [HideInInspector]
    public Transform activeTarget;
    public void Reset()
    {
        selectedSubroutine = Subroutine.None;
        activeTarget = null;
    }

    public void Awake()
    {
        Reset();
    }

    public void Update()
    {
        switch (selectedSubroutine)
        {
            case Subroutine.Attack:
                attackSubroutine.Do(this);
                break;
            case Subroutine.Dodge:
                dodgeSubroutine.Do(this);
                break;
            default:
                defaultSubroutine.Do(this);
                break;
        }
    }
}
