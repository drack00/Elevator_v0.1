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
    [HideInInspector]public Vector3 directionGizmo;
    void OnDrawGizmos() {
        Gizmos.DrawCube(destinationGizmo, Vector3.one);
        Gizmos.DrawLine(transform.position, directionGizmo);
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

                public bool Check(AIControlled_MovingObject mo)
                {
                    bool _check = haveTarget &&
                        belowHealthThreshold == (mo.health < healthThreshold) &&
                        belowStunThreshold == (mo.stun < stunThreshold);

                    if (_check && requireGroupApproval)
                    {
                        if (!string.IsNullOrEmpty(groupTag))
                        {
                            if (groupLayerMask != 0)
                                _check = AIGroup.GetApproval(mo, groupTag, groupLayerMask);
                            else
                                _check = AIGroup.GetApproval(mo, groupTag);
                        }
                        else
                        {
                            if (groupLayerMask != 0)
                                _check = AIGroup.GetApproval(mo, groupLayerMask);
                            else
                                _check = AIGroup.GetApproval(mo);
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
                                AIGroup.RemoveApproval(mo, groupTag, groupLayerMask);
                            else
                                AIGroup.RemoveApproval(mo, groupTag);
                        }
                        else
                        {
                            if (groupLayerMask != 0)
                                AIGroup.RemoveApproval(mo, groupLayerMask);
                            else
                                AIGroup.RemoveApproval(mo);
                        }
                    }

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
        public class Default : ISubroutine
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
            public class Patrol : ISubroutine
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

                    mo.agent.SetDestination(targetPoint.position);
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

                    mo.agent.SetDestination(destination);
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

                    mo.agent.SetDestination(targetPosition);
                }
            }
            public Idle idleSubroutine;
        }
        public Default defaultSubroutine;

        //movement
        [System.Serializable]
        public class Movement : ISubroutine
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
            public class FarApproach : ISubroutine
            {
                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 targetVector = (mo.transform.position - target.position).normalized;
                    Vector3 destination = (targetVector * mo.lowLevelAIRoutine.movementSubroutine.farCutoff) + target.position;

                    mo.agent.SetDestination(destination);
                }
            }
            public FarApproach farApproachSubroutine;

            public float nearCutoff;
            [System.Serializable]
            public class NearApproach : ISubroutine
            {
                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 targetVector = (mo.transform.position - target.position).normalized;
                    Vector3 destination = (targetVector * mo.lowLevelAIRoutine.movementSubroutine.nearCutoff) + target.position;

                    mo.agent.SetDestination(destination);
                }
            }
            public NearApproach nearApproachSubroutine;

            public bool stayAtRange;
            [System.Serializable]
            public class AtRange : ISubroutine
            {
                public void Do(AIControlled_MovingObject mo)
                {
                    Transform target = mo.highLevelAIRoutine.activeTarget;
                    Vector3 rangeVector = (mo.transform.position - target.position).normalized;
                    Vector3 destination = (rangeVector * mo.lowLevelAIRoutine.movementSubroutine.farCutoff) + target.position;

                    mo.agent.SetDestination(destination);
                }
            }
            public AtRange atRangeSubroutine;

            [System.Serializable]
            public class Flank : ISubroutine
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

                    mo.agent.SetDestination(destination);
                }
            }
            public Flank flankSubroutine;
        }
        public Movement movementSubroutine;

        //attack
        [System.Serializable]
        public class Attack : ISubroutine
        {
            public void Do(AIControlled_MovingObject mo)
            {
                mo.agent.SetDestination(mo.transform.position);

                mo.animator.SetTrigger("Attack");
            }
        }
        public Attack attackSubroutine;

        //avoidance
        [System.Serializable]
        public class Avoid : ISubroutine
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
            public class Flee : ISubroutine
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
            public class Dodge : ISubroutine
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

    public virtual void Awake ()
    {
        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.Stop();
    }

    public virtual void FixedUpdate()
    {
        highLevelAIRoutine.Update(this);

        destinationGizmo = agent.destination;

        GroundCheck();
        Vector3 navDirection = (agent.nextPosition - transform.position).normalized;
        movementSettings.UpdateDesiredTargetSpeed(navDirection);

        if ((Mathf.Abs(navDirection.x) > float.Epsilon || Mathf.Abs(navDirection.z) > float.Epsilon || Mathf.Abs(navDirection.y) > float.Epsilon) && 
            (advancedSettings.airControl || m_IsGrounded))
        {
            Vector3 desiredMove = new Vector3(
                navDirection.x * movementSettings.CurrentTargetSpeed,
                navDirection.y * movementSettings.CurrentTargetSpeed,
                navDirection.z * movementSettings.CurrentTargetSpeed);

            if (rigidbody.velocity.sqrMagnitude < (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                rigidbody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
        }
    }

    [System.Serializable]
    public class MovementSettings
    {
        public float ForwardSpeed = 8.0f;
        public float BackwardSpeed = 4.0f;
        public float StrafeSpeed = 4.0f;
        public float JumpForce = 30f;
        public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
        [HideInInspector]public float CurrentTargetSpeed = 8f;

        public void UpdateDesiredTargetSpeed(Vector3 input)
        {
            if (input == Vector3.zero) return;
            if (input.x > 0 || input.x < 0)
                CurrentTargetSpeed = StrafeSpeed;
            if (input.y < 0)
                CurrentTargetSpeed = BackwardSpeed;
            if (input.y > 0)
                CurrentTargetSpeed = ForwardSpeed;
        }
    }
    public MovementSettings movementSettings = new MovementSettings();
    [System.Serializable]
    public class AdvancedSettings
    {
        public float groundCheckDistance = 0.01f;
        public float stickToGroundHelperDistance = 0.5f;
        public float slowDownRate = 20f;
        public bool airControl;
    }
    public AdvancedSettings advancedSettings = new AdvancedSettings();
    private CapsuleCollider m_Capsule
    {
        get
        {
            return GetComponent<CapsuleCollider>();
        }
    }
    private Vector3 m_GroundContactNormal;
    private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
    private void GroundCheck()
    {
        m_PreviouslyGrounded = m_IsGrounded;
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, m_Capsule.radius, Vector3.down, out hitInfo, ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance))
        {
            m_IsGrounded = true;
            m_GroundContactNormal = hitInfo.normal;
        }
        else
        {
            m_IsGrounded = false;
            m_GroundContactNormal = Vector3.up;
        }
        if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            m_Jumping = false;
    }
    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
        return movementSettings.SlopeCurveModifier.Evaluate(angle);
    }
}
 
 