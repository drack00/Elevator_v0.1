using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Action : MonoBehaviour {
    [System.Serializable]
    public enum Subroutine
    {
        Masked, None, HaveTarget, Targeted
    }

    public interface ISubroutine
    {
        void Do(AI_Action action);
    }

    [System.Serializable]
    public class Default : ISubroutine
    {
        public void Do (AI_Action action)
        {

        }
    }
    public Default defaultSubroutine;

    [HideInInspector]
    public Subroutine selectedSubroutine;
    [HideInInspector]
    public Transform activeTarget;

    public void Awake()
    {
        selectedSubroutine = Subroutine.None;
    }
    public void FixedUpdate()
    {
        ISubroutine activeSubroutine = defaultSubroutine;

        switch (selectedSubroutine)
        {
            
        }

        activeSubroutine.Do(this);
    }
}
