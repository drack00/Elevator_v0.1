using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class AI_Movement : MonoBehaviour
{
    [System.Serializable]
    public enum Subroutine
    {
        Masked, None, HaveTarget, Targeted
    }

    public interface ISubroutine
    {
        void Do(AI_Movement movement);
    }

    public NavMeshAgent agent
    {
        get
        {
            return GetComponent<NavMeshAgent>();
        }
    }

    //default
    [System.Serializable]
    public class Default : ISubroutine
    {
        [System.Serializable]
        public enum Type
        {
            Patrol, Flock, Idle
        }
        public Type type;

        public void Do(AI_Movement movement)
        {
            switch (type)
            {
                case Type.Patrol:
                    patrolSubroutine.Do(movement);
                    break;
                case Type.Flock:
                    flockSubroutine.Do(movement);
                    break;
                case Type.Idle:
                    idleSubroutine.Do(movement);
                    break;
            }
        }

        [System.Serializable]
        public class Patrol : ISubroutine
        {
            public LayerMask patrolLayerMask;
            private Transform[] patrolTransforms
            {
                get
                {
                    List<Transform> _patrolTransforms = new List<Transform>();
                    foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                    {
                        if ((go.layer & patrolLayerMask) != 0)
                            _patrolTransforms.Add(go.transform);
                    }
                    return _patrolTransforms.ToArray();
                }
            }
            private int currentPatrolIndex;
            private int nextPatrolIndex
            {
                get
                {
                    int _nextPatrolIndex = currentPatrolIndex + 1;
                    if (_nextPatrolIndex > patrolTransforms.Length)
                        _nextPatrolIndex = 0;
                    return _nextPatrolIndex;
                }
            }
            private Transform targetPoint
            {
                get
                {
                    return patrolTransforms[nextPatrolIndex];
                }
            }
            public float patrolTolerance;

            public void Do(AI_Movement movement)
            {
                float distance = Vector3.Distance(movement.agent.transform.position, targetPoint.position);

                if (distance < patrolTolerance)
                    currentPatrolIndex = nextPatrolIndex;

                movement.SetDestination(targetPoint.position);
            }
        }
        public Patrol patrolSubroutine;
        [System.Serializable]
        public class Flock : ISubroutine
        {
            public LayerMask flockingLayerMask;
            private Transform[] flockingTransforms
            {
                get
                {
                    List<Transform> _flockingTransforms = new List<Transform>();
                    foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                    {
                        if ((go.layer & flockingLayerMask) != 0)
                            _flockingTransforms.Add(go.transform);
                    }
                    return _flockingTransforms.ToArray();
                }
            }
            public Vector3 flockOffset;

            public void Do(AI_Movement movement)
            {
                Vector3 targetPosition = MathStuff.GetClosestTransform(movement.agent.transform.position, flockingTransforms).position;
                Vector3 _flockOffset = Quaternion.LookRotation((targetPosition - movement.agent.transform.position).normalized) * flockOffset;
                Vector3 destination = targetPosition + _flockOffset;

                movement.SetDestination(destination);
            }
        }
        public Flock flockSubroutine;
        [System.Serializable]
        public class Idle : ISubroutine
        {
            private bool centerSet;
            private Vector3 centerPosition;
            private bool targetSet;
            private Vector3 targetPosition;
            public void Reset() { targetSet = false; centerSet = false; }
            public Vector3 maxStray;

            public void Do(AI_Movement movement)
            {
                if (!centerSet)
                {
                    centerPosition = movement.agent.transform.position;
                    centerSet = true;
                }

                if (!targetSet || movement.agent.transform.position == targetPosition)
                {
                    targetPosition = (new Vector3(Random.Range(-1 * maxStray.x, maxStray.x), Random.Range(-1 * maxStray.y, maxStray.y), Random.Range(-1 * maxStray.z, maxStray.z))) + centerPosition;
                    targetSet = true;
                }

                movement.SetDestination(targetPosition);
            }
        }
        public Idle idleSubroutine;
    }
    public Default defaultSubroutine;

    //havetarget
    [System.Serializable]
    public class HaveTarget : ISubroutine
    {
        public void Do(AI_Movement movement)
        {
            Transform target = movement.activeTarget;
            float distance = Vector3.Distance(movement.agent.transform.position, target.position);

            if (distance <= farCutoff)
            {
                if (!stayAtRange)
                {
                    if (distance <= nearCutoff)
                        flankSubroutine.Do(movement);
                    else
                        nearApproachSubroutine.Do(movement);
                }
                else
                    atRangeSubroutine.Do(movement);
            }
            else
                farApproachSubroutine.Do(movement);
        }

        public float farCutoff;
        [System.Serializable]
        public class FarApproach : ISubroutine
        {
            public void Do(AI_Movement movement)
            {
                Transform target = movement.activeTarget;
                Vector3 targetVector = (movement.agent.transform.position - target.position).normalized;
                Vector3 destination = (targetVector * movement.haveTargetSubroutine.farCutoff) + target.position;

                movement.SetDestination(destination);
            }
        }
        public FarApproach farApproachSubroutine;

        public float nearCutoff;
        [System.Serializable]
        public class NearApproach : ISubroutine
        {
            public void Do(AI_Movement movement)
            {
                Transform target = movement.activeTarget;
                Vector3 targetVector = (movement.agent.transform.position - target.position).normalized;
                Vector3 destination = (targetVector * movement.haveTargetSubroutine.nearCutoff) + target.position;

                movement.SetDestination(destination);
            }
        }
        public NearApproach nearApproachSubroutine;

        public bool stayAtRange;
        [System.Serializable]
        public class AtRange : ISubroutine
        {
            public void Do(AI_Movement movement)
            {
                Transform target = movement.activeTarget;
                Vector3 rangeVector = (movement.agent.transform.position - target.position).normalized;
                Vector3 destination = (rangeVector * movement.haveTargetSubroutine.farCutoff) + target.position;

                movement.SetDestination(destination);
            }
        }
        public AtRange atRangeSubroutine;

        [System.Serializable]
        public class Flank : ISubroutine
        {
            public float flankRadius;
            public float minDistance;

            public void Do(AI_Movement movement)
            {
                Transform target = movement.activeTarget;
                Vector3 rangeVector = (movement.agent.transform.position - target.position).normalized;

                float navAngle = Mathf.Atan2(flankRadius, minDistance) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0.0f, navAngle, 0.0f);

                Vector3 destination = (rangeVector * flankRadius) + target.position;
                destination = MathStuff.RotateAroundPivot(destination, target.position, rotation);

                movement.SetDestination(destination);
            }
        }
        public Flank flankSubroutine;
    }
    public HaveTarget haveTargetSubroutine;

    //targeted
    [System.Serializable]
    public class Targeted : ISubroutine
    {
        public void Do(AI_Movement movement)
        {
            GameObject target = movement.activeTarget.gameObject;

            if ((LayerMask.GetMask(LayerMask.LayerToName(target.layer)) & fleeLayerMask) != 0)
                fleeSubroutine.Do(movement);
        }

        public LayerMask fleeLayerMask;
        [System.Serializable]
        public class Flee : ISubroutine
        {
            public float minDistance;
            public float duration;
            private float _duration;
            public void Reset() { _duration = 0.0f; }

            public void Do(AI_Movement movement)
            {
                if (Mathf.Approximately(_duration, Mathf.Epsilon))
                {
                    _duration = duration;
                }
                else
                {
                    Transform target = movement.activeTarget;
                    Vector3 rangeVector = (movement.agent.transform.position - target.position).normalized;
                    Vector3 destination = (rangeVector * minDistance) + movement.agent.transform.position;

                    movement.agent.SetDestination(destination);

                    _duration -= Time.deltaTime;
                    if (_duration < 0.0f)
                        _duration = 0.0f;
                }
            }
        }
        public Flee fleeSubroutine;
    }
    public Targeted targetedSubroutine;

    public bool stickToMesh;
    public void SetDestination (Vector3 destination)
    {
        if (stickToMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(destination, out hit, agent.height * 2.0f, agent.areaMask))
                agent.SetDestination(hit.position);
        }
        else
            agent.SetDestination(destination);
    }

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
            case Subroutine.HaveTarget:
                haveTargetSubroutine.Do(this);
                break;
            case Subroutine.Targeted:
                targetedSubroutine.Do(this);
                break;
            default:
                defaultSubroutine.Do(this);
                break;
        }
    }
}
