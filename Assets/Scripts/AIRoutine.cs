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

	//default
	[System.Serializable]
	public struct Default : ISubroutine
    {
        public enum DefaultType
        {  
            Patrol, Flock, Idle
        }
        public DefaultType defaultType;

		public void Do (AIControlled_MovingObject mo)
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

			public void Do (AIControlled_MovingObject mo)
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

			public void Do (AIControlled_MovingObject mo)
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

			public void Do (AIControlled_MovingObject mo)
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
        public Transform target
        {
            get
            {
                return GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

        public void Do(AIControlled_MovingObject mo)
        {
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
			public void Do (AIControlled_MovingObject mo)
            {
                Transform target = mo.movementSubroutine.target;
                Vector3 targetVector = (mo.transform.position - target.position).normalized;
                Vector3 destination = (targetVector * mo.movementSubroutine.farCutoff) + target.position;
                mo.destinationGizmo = destination;

                mo.agent.SetDestination(destination);
			}
		}
		public FarApproach farApproachSubroutine;

        public float nearCutoff;
        [System.Serializable]
		public struct NearApproach : ISubroutine
        {
			public void Do (AIControlled_MovingObject mo)
            {
                Transform target = mo.movementSubroutine.target;
                Vector3 targetVector = (mo.transform.position - target.position).normalized;
                Vector3 destination = (targetVector * mo.movementSubroutine.nearCutoff) + target.position;
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
                Transform target = mo.movementSubroutine.target;
                Vector3 rangeVector = (mo.transform.position - target.position).normalized;
                Vector3 destination = (rangeVector * mo.movementSubroutine.farCutoff) + target.position;
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
                Transform target = mo.movementSubroutine.target;
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
        public Transform target
        {
            get
            {
                return GameObject.FindGameObjectWithTag("Player").transform;
            }
        }

        private bool meleeLastFrame;

        public void Do(AIControlled_MovingObject mo)
        {
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

            if(meleeFrame)
                meleeSubroutine.Do(mo);
            else if(distance < rangedMax && rangedMin < distance)
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
        public GameObject target
        {
            get
            {
                if (fleeTargets.Length < 1)
                    return fleeTargets[0];
                else if (dodgeTargets.Length < 1)
                    return dodgeTargets[0];

                return null;
            }
        }

        public void Do (AIControlled_MovingObject mo)
        {
            if (target == null)
                return;

            if ((target.layer & fleeLayerMask) != 0)
                fleeSubroutine.Do(mo);
            else if ((target.layer & dodgeLayerMask) != 0)
                dodgeSubroutine.Do(mo);
		}

        public LayerMask fleeLayerMask;
        private GameObject[] fleeTargets
        {
            get
            {
                List<GameObject> _fleeTargets = new List<GameObject>();
                foreach (GameObject go in FindObjectsOfType<GameObject>())
                {
                    if ((go.layer & fleeLayerMask) != 0)
                        _fleeTargets.Add(go);
                }
                return _fleeTargets.ToArray();
            }
        }
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
                    Transform target = mo.movementSubroutine.target;
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
        private GameObject[] dodgeTargets
        {
            get
            {
                List<GameObject> _dodgeTargets = new List<GameObject>();
                foreach (GameObject go in FindObjectsOfType<GameObject>())
                {
                    if ((go.layer & dodgeLayerMask) != 0)
                        _dodgeTargets.Add(go);
                }
                return _dodgeTargets.ToArray();
            }
        }
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
                    Transform target = mo.movementSubroutine.target;
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

    public ISubroutine activeSubroutine;

	public override void Start ()
    {
        base.Start();

		activeSubroutine = defaultSubroutine;
	} 

	public override void Update ()
    {
        base.Update();

		activeSubroutine.Do (this);
	}
}