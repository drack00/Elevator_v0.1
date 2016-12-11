using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AI : MonoBehaviour {
    public interface ICanCheck
    {
        float GetHealth();
        float GetStun();
        Animator GetAnimator();
    }
    [HideInInspector]
    public ICanCheck canCheck
    {
        get
        {
            return GetComponent<ICanCheck>();
        }
    }

    public interface ISubroutine
    {
        void Do(AI ai);
    }

    public NavMeshAgent agent;

    [System.Serializable]
    public struct HighLevelAI
    {
        [System.Serializable]
        public struct Profile
        {
            [System.Serializable]
            public enum Subroutine
            {
                Movement, Attack, Avoid
            }
            public Subroutine subroutine;

            public int priority;

            [System.Serializable]
            public class Conditions
            {
                [System.Serializable]
                public enum FindTargetMethod
                {
                    Collider, Camera, Ambient, None
                }
                public FindTargetMethod findTargetMethod;
                public bool findTargetByLayerMask;
                public string targetsTag;
                public LayerMask targetsLayerMask;
                private GameObject[] targets
                {
                    get
                    {
                        List<GameObject> _targets = new List<GameObject>();
                        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                        {
                            if (findTargetByLayerMask)
                            {
                                if ((go.layer & targetsLayerMask) != 0)
                                    _targets.Add(go);
                            }
                            else if (go.tag == targetsTag)
                                _targets.Add(go);
                        }
                        return _targets.ToArray();
                    }
                }
                public Collider targetsCollider;
                public Camera targetsCamera;
                private bool haveTarget
                {
                    get
                    {
                        bool _haveTarget = false;
                        switch (findTargetMethod)
                        {
                            case FindTargetMethod.Collider:
                                foreach (GameObject go in targets)
                                {
                                    if (targetsCollider.bounds.Contains(go.transform.position)) _haveTarget = true;
                                }
                                break;
                            case FindTargetMethod.Camera:
                                foreach (GameObject go in targets)
                                {
                                    if (GeometryUtility.TestPlanesAABB(
                                        GeometryUtility.CalculateFrustumPlanes(targetsCamera),
                                        go.GetComponent<Renderer>().bounds))
                                        _haveTarget = true;
                                }
                                break;
                            case FindTargetMethod.Ambient:
                                _haveTarget = targets.Length > 0;
                                break;
                            case FindTargetMethod.None:
                                _haveTarget = true;
                                break;
                        }
                        return _haveTarget;
                    }
                }
                public Transform activeTarget
                {
                    get
                    {
                        if (targets.Length == 0)
                            return null;
                        return targets[0].transform;
                    }
                }

                public bool requireGroupApproval;
                public string groupTag;
                public LayerMask groupLayerMask;

                public bool belowHealthThreshold;
                public float healthThreshold;
                public bool belowStunThreshold;
                public float stunThreshold;

                private float _warmup;
                public float warmup;
                private float _cooldown;
                public float cooldown;
                private float _delay;
                public float delay;

                public bool Check(AI ai)
                {
                    ICanCheck canCheck = ai.canCheck;

                    bool _check = haveTarget &&
                        belowHealthThreshold == (canCheck.GetHealth() < healthThreshold) &&
                        belowStunThreshold == (canCheck.GetStun() < stunThreshold);

                    if (_check && requireGroupApproval)
                    {
                        if (!string.IsNullOrEmpty(groupTag))
                        {
                            if (groupLayerMask != 0)
                                _check = AIGroup.GetApproval(canCheck, groupTag, groupLayerMask);
                            else
                                _check = AIGroup.GetApproval(canCheck, groupTag);
                        }
                        else
                        {
                            if (groupLayerMask != 0)
                                _check = AIGroup.GetApproval(canCheck, groupLayerMask);
                            else
                                _check = AIGroup.GetApproval(canCheck);
                        }
                    }

                    if (_check)
                    {
                        _warmup += Time.deltaTime;
                        if (_warmup > warmup)
                            _warmup = warmup;
                    }
                    else
                    {
                        _warmup = 0.0f;

                        if (_cooldown > 0.0f)
                        {
                            _cooldown -= Time.deltaTime;
                            if (_cooldown < 0.0f)
                                _cooldown = 0.0f;

                            _check = true;
                        }
                    }

                    _check = _check && Mathf.Approximately(_warmup, warmup);

                    if (_check && Mathf.Approximately(_cooldown, Mathf.Epsilon))
                        _cooldown = cooldown;

                    if (_check)
                    {
                        if (Mathf.Approximately(_delay, Mathf.Epsilon))
                            _delay = delay;
                        else
                        {
                            _check = false;

                            _delay -= Time.deltaTime;
                            if (_delay < 0.0f)
                                _delay = 0.0f;
                        }
                    }

                    if (!_check)
                    {
                        if (!string.IsNullOrEmpty(groupTag))
                        {
                            if (groupLayerMask != 0)
                                AIGroup.RemoveApproval(canCheck, groupTag, groupLayerMask);
                            else
                                AIGroup.RemoveApproval(canCheck, groupTag);
                        }
                        else
                        {
                            if (groupLayerMask != 0)
                                AIGroup.RemoveApproval(canCheck, groupLayerMask);
                            else
                                AIGroup.RemoveApproval(canCheck);
                        }
                    }

                    return _check;
                }
            }
            public Conditions conditions;
        }
        public Profile[] profiles;
        [HideInInspector]
        public Transform activeTarget;

        public void Update(AI ai)
        {
            ISubroutine activeSubroutine = ai.lowLevelAIRoutine.defaultSubroutine;

            List<Profile> _profiles = new List<Profile>(profiles);
            _profiles.Sort((Profile x, Profile y) => { return x.priority.CompareTo(y.priority); });
            for (int i = 0; i < _profiles.Count; i++)
            {
                if (_profiles[i].conditions.Check(ai))
                {
                    activeTarget = _profiles[i].conditions.activeTarget;
                    switch (_profiles[i].subroutine)
                    {
                        case Profile.Subroutine.Movement:
                            activeSubroutine = ai.lowLevelAIRoutine.movementSubroutine;
                            break;
                        case Profile.Subroutine.Attack:
                            activeSubroutine = ai.lowLevelAIRoutine.attackSubroutine;
                            break;
                        case Profile.Subroutine.Avoid:
                            activeSubroutine = ai.lowLevelAIRoutine.avoidSubroutine;
                            break;
                    }
                }
            }

            activeSubroutine.Do(ai);
        }
    }
    public HighLevelAI highLevelAIRoutine;

    [System.Serializable]
    public struct LowLevelAI
    {
        //default
        [System.Serializable]
        public class Default : ISubroutine
        {
            public enum DefaultType
            {
                Patrol, Flock, Idle
            }
            public DefaultType defaultType;

            public void Do(AI ai)
            {
                switch (defaultType)
                {
                    case DefaultType.Patrol:
                        patrolSubroutine.Do(ai);
                        break;
                    case DefaultType.Flock:
                        flockSubroutine.Do(ai);
                        break;
                    case DefaultType.Idle:
                        idleSubroutine.Do(ai);
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

                public void Do(AI ai)
                {
                    float distance = Vector3.Distance(ai.agent.transform.position, targetPoint.position);

                    if (distance < patrolTolerance)
                        currentPatrolIndex = nextPatrolIndex;

                    ai.agent.SetDestination(targetPoint.position);
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

                public void Do(AI ai)
                {
                    Vector3 targetPosition = MathStuff.GetClosestTransform(ai.agent.transform.position, flockingTransforms).position;
                    Vector3 _flockOffset = Quaternion.LookRotation((targetPosition - ai.agent.transform.position).normalized) * flockOffset;
                    Vector3 destination = targetPosition + _flockOffset;

                    ai.agent.SetDestination(destination);
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

                public void Do(AI ai)
                {
                    if (!centerSet)
                    {
                        centerPosition = ai.agent.transform.position;
                        centerSet = true;
                    }

                    if (!targetSet || ai.agent.transform.position == targetPosition)
                    {
                        targetPosition = (new Vector3(Random.Range(-1 * maxStray.x, maxStray.x), Random.Range(-1 * maxStray.y, maxStray.y), Random.Range(-1 * maxStray.z, maxStray.z))) + centerPosition;
                        targetSet = true;
                    }

                    ai.agent.SetDestination(targetPosition);
                }
            }
            public Idle idleSubroutine;
        }
        public Default defaultSubroutine;

        //movement
        [System.Serializable]
        public class Movement : ISubroutine
        {
            public void Do(AI ai)
            {
                Transform target = ai.highLevelAIRoutine.activeTarget;
                float distance = Vector3.Distance(ai.agent.transform.position, target.position);

                if (distance <= farCutoff)
                {
                    if (!stayAtRange)
                    {
                        if (distance <= nearCutoff)
                            flankSubroutine.Do(ai);
                        else
                            nearApproachSubroutine.Do(ai);
                    }
                    else
                        atRangeSubroutine.Do(ai);
                }
                else
                    farApproachSubroutine.Do(ai);
            }

            public float farCutoff;
            [System.Serializable]
            public class FarApproach : ISubroutine
            {
                public void Do(AI ai)
                {
                    Transform target = ai.highLevelAIRoutine.activeTarget;
                    Vector3 targetVector = (ai.agent.transform.position - target.position).normalized;
                    Vector3 destination = (targetVector * ai.lowLevelAIRoutine.movementSubroutine.farCutoff) + target.position;

                    ai.agent.SetDestination(destination);
                }
            }
            public FarApproach farApproachSubroutine;

            public float nearCutoff;
            [System.Serializable]
            public class NearApproach : ISubroutine
            {
                public void Do(AI ai)
                {
                    Transform target = ai.highLevelAIRoutine.activeTarget;
                    Vector3 targetVector = (ai.agent.transform.position - target.position).normalized;
                    Vector3 destination = (targetVector * ai.lowLevelAIRoutine.movementSubroutine.nearCutoff) + target.position;

                    ai.agent.SetDestination(destination);
                }
            }
            public NearApproach nearApproachSubroutine;

            public bool stayAtRange;
            [System.Serializable]
            public class AtRange : ISubroutine
            {
                public void Do(AI ai)
                {
                    Transform target = ai.highLevelAIRoutine.activeTarget;
                    Vector3 rangeVector = (ai.agent.transform.position - target.position).normalized;
                    Vector3 destination = (rangeVector * ai.lowLevelAIRoutine.movementSubroutine.farCutoff) + target.position;

                    ai.agent.SetDestination(destination);
                }
            }
            public AtRange atRangeSubroutine;

            [System.Serializable]
            public class Flank : ISubroutine
            {
                public float flankRadius;
                public float minDistance;

                public void Do(AI ai)
                {
                    Transform target = ai.highLevelAIRoutine.activeTarget;
                    Vector3 rangeVector = (ai.agent.transform.position - target.position).normalized;

                    float navAngle = Mathf.Atan2(flankRadius, minDistance) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0.0f, navAngle, 0.0f);

                    Vector3 destination = (rangeVector * flankRadius) + target.position;
                    destination = MathStuff.RotateAroundPivot(destination, target.position, rotation);

                    ai.agent.SetDestination(destination);
                }
            }
            public Flank flankSubroutine;
        }
        public Movement movementSubroutine;

        //attack
        [System.Serializable]
        public class Attack : ISubroutine
        {
            public void Do(AI ai)
            {
                ai.agent.SetDestination(ai.agent.transform.position);

                ai.canCheck.GetAnimator().SetTrigger("Attack");
            }
        }
        public Attack attackSubroutine;

        //avoidance
        [System.Serializable]
        public class Avoid : ISubroutine
        {
            public void Do(AI ai)
            {
                GameObject target = ai.highLevelAIRoutine.activeTarget.gameObject;

                if ((target.layer & fleeLayerMask) != 0)
                    fleeSubroutine.Do(ai);
                else if ((target.layer & dodgeLayerMask) != 0)
                    dodgeSubroutine.Do(ai);
            }

            public LayerMask fleeLayerMask;
            [System.Serializable]
            public class Flee : ISubroutine
            {
                public float minDistance;
                public float duration;
                private float _duration;
                public void Reset() { _duration = 0.0f; }

                public void Do(AI ai)
                {
                    if (Mathf.Approximately(_duration, Mathf.Epsilon))
                    {
                        _duration = duration;
                    }
                    else
                    {
                        Transform target = ai.highLevelAIRoutine.activeTarget;
                        Vector3 rangeVector = (ai.agent.transform.position - target.position).normalized;
                        Vector3 destination = (rangeVector * minDistance) + ai.agent.transform.position;

                        ai.agent.SetDestination(destination);

                        _duration -= Time.deltaTime;
                        if (_duration < 0.0f)
                            _duration = 0.0f;
                    }
                }
            }
            public Flee fleeSubroutine;

            public LayerMask dodgeLayerMask;
            [System.Serializable]
            public class Dodge : ISubroutine
            {
                public float distance;
                public float cooldown;
                private float _cooldown;
                public void Reset() { _cooldown = 0.0f; }

                public void Do(AI ai)
                {
                    if (Mathf.Approximately(_cooldown, Mathf.Epsilon))
                    {
                        Transform target = ai.highLevelAIRoutine.activeTarget;
                        Vector3 rangeVector = (ai.agent.transform.position - target.position).normalized;
                        Vector3 destination = (rangeVector * distance) + ai.agent.transform.position;

                        ai.agent.SetDestination(destination);

                        _cooldown = cooldown;
                    }
                    else
                    {
                        _cooldown -= Time.deltaTime;
                        if (_cooldown < 0.0f)
                            _cooldown = 0.0f;
                    }
                }
            }
            public Dodge dodgeSubroutine;
        }
        public Avoid avoidSubroutine;
    }
    public LowLevelAI lowLevelAIRoutine;

    public void OnAwake()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.Stop();
    }
    public void OnUpdate()
    {
        highLevelAIRoutine.Update(this);
    }
}
