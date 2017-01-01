using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Orientation : MonoBehaviour
{
    [System.Serializable]
    public enum Subroutine
    {
        Masked, None, FaceDirection, FaceTarget
    }

    public interface ISubroutine
    {
        void Do(AI_Orientation orientation);
    }

    public Transform root;

    [System.Serializable]
    public class Default : ISubroutine
    {
        public void Do(AI_Orientation orientation)
        {

        }
    }
    public Default defaultSubroutine;

    [System.Serializable]
    public class FaceDirection : ISubroutine
    {
        public Vector3 direction;

        public void Do(AI_Orientation orientation)
        {
            orientation.root.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }
    public FaceDirection faceDirection;

    [System.Serializable]
    public class FaceTarget : ISubroutine
    {
        public Vector3 offset;

        public void Do(AI_Orientation orientation)
        {
            Vector3 navDirection = (orientation.activeTarget.position - orientation.root.position).normalized;

            orientation.root.rotation = Quaternion.LookRotation(navDirection);
        }
    }
    public FaceTarget faceTarget;

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
            case Subroutine.FaceDirection:
                activeSubroutine = faceDirection;
                break;
            case Subroutine.FaceTarget:
                activeSubroutine = faceTarget;
                break;
        }

        activeSubroutine.Do(this);
    }
}