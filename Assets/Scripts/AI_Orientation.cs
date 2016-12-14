using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AI_Orientation : MonoBehaviour {
    [System.Serializable]
    public enum Subroutine
    {
        None = 0, Masked, HaveTarget, Targeted
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
    public class HaveTarget : ISubroutine
    {
        [System.Serializable]
        public enum Type
        {
            FaceDirection, FaceTarget
        }
        public Type type;

        public void Do (AI_Orientation orientation)
        {
            switch (type)
            {
                case Type.FaceDirection:
                    faceDirection.Do(orientation);
                    break;
                case Type.FaceTarget:
                    faceTarget.Do(orientation);
                    break;
            }
        }

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
    }
    public HaveTarget haveTargetSubroutine;

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
            case Subroutine.None:
                activeSubroutine = null;
                break;
            case Subroutine.HaveTarget:
                activeSubroutine = haveTargetSubroutine;
                break;
        }

        activeSubroutine.Do(this);
    }
}