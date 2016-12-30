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