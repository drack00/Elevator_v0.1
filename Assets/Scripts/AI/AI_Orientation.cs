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

    public Quaternion desiredRotation;

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
            orientation.desiredRotation = Quaternion.LookRotation(direction.normalized);
        }
    }
    public FaceDirection faceDirection;

    [System.Serializable]
    public class FaceTarget : ISubroutine
    {
        public Vector3 offset;

        public void Do(AI_Orientation orientation)
        {
            Vector3 navDirection = (orientation.activeTarget.position - orientation.transform.position).normalized;

            orientation.desiredRotation = Quaternion.LookRotation(navDirection);
        }
    }
    public FaceTarget faceTarget;

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
            case Subroutine.FaceDirection:
                faceDirection.Do(this);
                break;
            case Subroutine.FaceTarget:
                faceTarget.Do(this);
                break;
            default:
                defaultSubroutine.Do(this);
                break;
        }
    }
}