using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIControlled_MovingObject : MovingObject
{
	public interface ISubroutine
    {
		void Do (AIControlled_MovingObject mo);
	}

    public NavMeshAgent agent
    {
        get
        {
            return GetComponent<NavMeshAgent>();
        }
    }

    [HideInInspector]public Vector3 destinationGizmo;
    void OnDrawGizmos() {
        Gizmos.DrawWireCube(destinationGizmo, Vector3.one);
    }

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
            public struct Conditions
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
                        foreach (GameObject go in FindObjectsOfType<GameObject>())
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
                                        go.GetComponent<Renderer>().bounds)) _haveTarget = true;
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

                public bool belowThreshold;
                public float healthThreshold;

                private float _delay;
                public float delay;
                private float _cooldown;
                public float cooldown;

                public bool Check(AIControlled_MovingObject mo)
                {
                    bool _check = haveTarget && belowThreshold == (mo.health < healthThreshold);

                    if (_check)
                    {
                        _delay += Time.deltaTime;
                        if (_delay > delay)
                            _delay = delay; 
                    }
                    else if (_cooldown > 0.0f)
                    { 
                        _cooldown -= Time.deltaTime;
                        if (_cooldown < 0.0f)
                            _cooldown = 0.0f;

                        return true;
                    }

                    _check = _check && Mathf.Approximately(_delay, delay);

                    if (_check)
                        _cooldown = cooldown;

                    return _check;
                }
            }
            public Conditions conditions;
        }
        public Profile[] profiles;
        [HideInInspector]public Transform activeTarget;

        public void Update (AIControlled_MovingObject mo)
        {
            ISubroutine activeSubroutine = mo.lowLevelAIRoutine.defaultSubroutine;

            List<Profile> _profiles = new List<Profile>(profiles);
            _profiles.Sort((Profile x, Profile y) => { return x.priority.CompareTo(y.priority); });
            for (int i = 0; i < _profiles.Count; i++)
            {
                if (_profiles[i].conditions.Check(mo))
                {
                    activeTarget = _profiles[i].conditions.activeTarget;
                    switch (_profiles[i].subroutine)
                    {
                        case Profile.Subroutine.Movement:
                            activeSubroutine = mo.lowLevelAIRoutine.movementSubroutine;
                            break;
                        case Profile.Subroutine.Attack:
                            activeSubroutine = mo.lowLevelAIRoutine.attackSubroutine;
                            break;
                        case Profile.Subroutine.Avoid:
                            activeSubroutine = mo.lowLevelAIRoutine.avoidSubroutine;
                            break;
                    }
                }
            }

            activeSubroutine.Do(mo);
        }
    }
    public HighLevelAI highLevelAIRoutine;

    [System.Serializable]
    public struct LowLevelAI
    {
        //default
        [System.Serializable]
        public struct Default : ISubroutine
        {
            public enum DefaultType
            {
                Patrol, Flock, Idle
            }
            public DefaultType defaultType;

            public void Do(AIControlled_MovingObject mo)
            {
                switch (defaultType)
                {
                    case DefaultType.Patrol:
                        patrolSubroutine.Do(mo);
                        break;
                    case DefaultType.Flock:
                        flockSubroutine.Do(mo);
                        break;
                    case DefaultType.Idle:
                        idleSubroutine.Do(mo);
                        break;
                }
            }

            [System.Serializable]
            public struct Patrol : ISubroutine
            {
                public LayerMask patrolLayerMask;
                private Transform[] patrolTransforms
                {
                    get
                    {
                        List<Transform> _patrolTransforms = new List<Transform>();
                        foreach (GameObject go in FindObjectsOfType<GameObject>())
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

                public void Do(AIControlled_MovingObject mo)
                {
                    float distance = Vector3.Distance(mo.transform.position, targetPoint.position);

                    if (distance < patrolTolerance)
                        currentPatrolIndex = nextPatrolIndex;

                    mo.destinationGizmo = targetPoint.position;

                    mo.agent.SetDestination(targetPoint.position);
                }
            }
            public Patrol patrolSubroutine;
            [System.Serializable]
            public struct Flock : ISubroutine
            {
                public LayerMask flockingLayerMask;
                private Transform[] flockingTransforms
                {
                    get
                    {
                        List<Transform> _flockingTransforms = new List<Transform>();
                        foreach (GameObject go in FindObjectsOfType<GameObject>())
                        {
                            if ((go.layer & flockingLayerMask) != 0)
                                _flockingTransforms.Add(go.transform);
                        }
                        return _flockingTransforms.ToArray();
                    }
                }
                public Vector3 flockOffset;

                public void Do(AIControlled_MovingObject mo)
                {
                    Vector3 targetPosition = MathStuff.GetClosestTransform(mo.transform.position, flockingTransforms).position;
                    Vector3 _flockOffset = Quaternion.LookRotation((targetPosition - mo.transform.position).normalized) * flockOffset;
                    Vector3 destination = targetPosition + _flockOffset;
                    mo.destinationGizmo = destination;

                    mo.agent.SetDestination(destination);
                }
            }
            public Flock flockSubroutine;
            [System.Serializable]
            public struct Idle : ISubroutine
            {
                private bool centerSet;
                private Vector3 centerPosition;
                private bool targetSet;
                private Vector3 targetPosition;
                public void Reset() { targetSet = false; centerSet = false; }
                public Vector3 maxStray;

                public void Do(AIControlled_MovingObject mo)
                {
                    if (!centerSet)
                    {
                        centerPosition = mo.transform.position;
                        centerSet = true;
                    }

                    if (!targetSet || mo.transform.position == targetPosition)
                    {
                        targetPosition = (new Vector3(Random.Range(-1 * maxStray.x, maxStray.x), Random.Range(-1 * maxStray.y, maxStray.y), Random.Range(-1 * maxStray.z, maxStray.z))) + centerPosition;
                        targetSet = true;
                    }
                    mo.destinationGizmo = targetPosition;

                    mo.agent.SetDestination(targetPosition);
                }
            }
            public Idle idleSubroutine;
        }
        public Default defaultSubroutine;

        //movement
        [System.Serializable]
        public struct Movement : ISubroutine
        {
            public void Do(AIControlled_MovingObject mo)
            {
                Transform target = mo.highLevelAIRoutine.activeTarget;
                float distance = Vector3.Distance(mo.transform.position, target.position);

                if (distance <= farCutoff)
                {
                    if (!stayAtRange)
                    {
                        if (distance <= nearCutoff)
                            flankSubroutine.Do(mo);
                        else
                            nearApproachSubroutine.Do(mo);
                    }
                    else
                        atRangeSubroutine.Do(mo);
                }
                else
                    farApproachSubroutine.Do(mo);
            }

            public float farCutoff;
            [System.Serializable]
            public struct FarApproach : ISubroutine
            {
                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 targetVector = (mo.transform.position - target.position).normalized;
                    Vector3 destination = (targetVector * mo.lowLevelAIRoutine.movementSubroutine.farCutoff) + target.position;
                    mo.destinationGizmo = destination;

                    mo.agent.SetDestination(destination);
                }
            }
            public FarApproach farApproachSubroutine;

            public float nearCutoff;
            [System.Serializable]
            public struct NearApproach : ISubroutine
            {
                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 targetVector = (mo.transform.position - target.position).normalized;
                    Vector3 destination = (targetVector * mo.lowLevelAIRoutine.movementSubroutine.nearCutoff) + target.position;
                    mo.destinationGizmo = destination;

                    mo.agent.SetDestination(destination);
                }
            }
            public NearApproach nearApproachSubroutine;

            public bool stayAtRange;
            [System.Serializable]
            public struct AtRange : ISubroutine
            {
                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 rangeVector = (mo.transform.position - target.position).normalized;
                    Vector3 destination = (rangeVector * mo.lowLevelAIRoutine.movementSubroutine.farCutoff) + target.position;
                    mo.destinationGizmo = destination;

                    mo.agent.SetDestination(destination);
                }
            }
            public AtRange atRangeSubroutine;

            [System.Serializable]
            public struct Flank : ISubroutine
            {
                public float flankRadius;
                public Vector3 minDistances;

                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 rangeVector = (mo.transform.position - target.position).normalized;
                    float y = Mathf.Atan2(minDistances.x, flankRadius) * Mathf.Rad2Deg;
                    float z = Mathf.Atan2(minDistances.y, flankRadius) * Mathf.Rad2Deg;
                    float x = Mathf.Atan2(minDistances.z, flankRadius) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(x, y, z);
                    Vector3 destination = (rangeVector * flankRadius) + target.position;
                    destination = MathStuff.RotateAroundPivot(destination, target.position, rotation);
                    mo.destinationGizmo = destination;

                    mo.agent.SetDestination(destination);
                }
            }
            public Flank flankSubroutine;
        }
        public Movement movementSubroutine;

        //attack
        [System.Serializable]
        public struct Attack : ISubroutine
        {
            private bool meleeLastFrame;

            public void Do(AIControlled_MovingObject mo)
            {
                Transform target = mo.highLevelAIRoutine.activeTarget;
                float distance = Vector3.Distance(mo.transform.position, target.position);

                bool meleeFrame = false;

                if (distance < meleeMax && meleeMin < distance)
                    meleeFrame = true;
                if (distance < rangedMax && rangedMin < distance)
                    meleeFrame = false;

                if (meleeFrame != meleeLastFrame)
                {
                    meleeSubroutine.Reset();
                    rangedSubroutine.Reset();
                }

                if (meleeFrame)
                    meleeSubroutine.Do(mo);
                else if (distance < rangedMax && rangedMin < distance)
                    rangedSubroutine.Do(mo);

                meleeLastFrame = meleeFrame;
            }

            public float rangedMax;
            public float rangedMin;
            [System.Serializable]
            public struct Ranged : ISubroutine
            {
                public float attackCooldown;
                private float _attackCooldown;
                public void Reset() { _attackCooldown = 0.0f; }

                public void Do(AIControlled_MovingObject mo)
                {
                    if (Mathf.Approximately(_attackCooldown, Mathf.Epsilon))
                    {


                        _attackCooldown = attackCooldown;
                    }
                    else
                    {


                        _attackCooldown -= Time.deltaTime;
                        if (_attackCooldown < 0.0f)
                            _attackCooldown = 0.0f;
                    }
                }
            }
            public Ranged rangedSubroutine;

            public float meleeMax;
            public float meleeMin;
            [System.Serializable]
            public struct Melee : ISubroutine
            {
                public float attackCooldown;
                private float _attackCooldown;
                public void Reset() { _attackCooldown = 0.0f; }

                public void Do(AIControlled_MovingObject mo)
                {
                    if (Mathf.Approximately(_attackCooldown, Mathf.Epsilon))
                    {


                        _attackCooldown = attackCooldown;
                    }
                    else
                    {


                        _attackCooldown -= Time.deltaTime;
                        if (_attackCooldown < 0.0f)
                            _attackCooldown = 0.0f;
                    }
                }
            }
            public Melee meleeSubroutine;
        }
        public Attack attackSubroutine;

        //avoidance
        [System.Serializable]
        public struct Avoid : ISubroutine
        {
            public void Do(AIControlled_MovingObject mo)
            {
                GameObject target = mo.highLevelAIRoutine.activeTarget.gameObject;

                if ((target.layer & fleeLayerMask) != 0)
                    fleeSubroutine.Do(mo);
                else if ((target.layer & dodgeLayerMask) != 0)
                    dodgeSubroutine.Do(mo);
            }

            public LayerMask fleeLayerMask;
            [System.Serializable]
            public struct Flee : ISubroutine
            {
                public float minDistance;
                public float duration;
                private float _duration;
                public void Reset() { _duration = 0.0f; }

                public void Do(AIControlled_MovingObject mo)
                {
                    if (Mathf.Approximately(_duration, Mathf.Epsilon))
                    {
                        _duration = duration;
                    }
                    else
                    {
                        Transform target = mo.highLevelAIRoutine.activeTarget;
                        Vector3 rangeVector = (mo.transform.position - target.position).normalized;
                        Vector3 destination = (rangeVector * minDistance) + mo.transform.position;
                        mo.destinationGizmo = destination;

                        mo.agent.SetDestination(destination);

                        _duration -= Time.deltaTime;
                        if (_duration < 0.0f)
                            _duration = 0.0f;
                    }
                }
            }
            public Flee fleeSubroutine;

            public LayerMask dodgeLayerMask;
            [System.Serializable]
            public struct Dodge : ISubroutine
            {
                public float distance;
                public float cooldown;
                private float _cooldown;
                public void Reset() { _cooldown = 0.0f; }

                public void Do(AIControlled_MovingObject mo)
                {
                    if (Mathf.Approximately(_cooldown, Mathf.Epsilon))
                    {
                        Transform target = mo.highLevelAIRoutine.activeTarget;
                        Vector3 rangeVector = (mo.transform.position - target.position).normalized;
                        Vector3 destination = (rangeVector * distance) + mo.transform.position;
                        mo.destinationGizmo = destination;

                        mo.agent.SetDestination(destination);

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

	public override void Update ()
    {
        base.Update();

		highLevelAIRoutine.Update (this);
	}
}