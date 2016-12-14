using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AI_Movement))]
[RequireComponent(typeof(AI_Orientation))]
[RequireComponent(typeof(AI_Action))]
public class AI_Master : MonoBehaviour {
    public Profile.Conditions.ICanCheck canCheck
    {
        get
        {
            return GetComponent<Profile.Conditions.ICanCheck > ();
        }
    }

    public AI_Movement movement
    {
        get
        {
            return GetComponent<AI_Movement>();
        }
    }
    public AI_Orientation orientation
    {
        get
        {
            return GetComponent<AI_Orientation>();
        }
    }
    public AI_Action action
    {
        get
        {
            return GetComponent<AI_Action>();
        }
    }

    [System.Serializable]
    public struct Profile
    {
        public AI_Movement.Subroutine movementSubroutine;
        public AI_Orientation.Subroutine orientationSubroutine;
        public AI_Action.Subroutine actionSubroutine;

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

            public bool Check(AI_Master master)
            {
                ICanCheck canCheck = master.canCheck;

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

            public interface ICanCheck
            {
                float GetHealth();
                float GetStun();
                Animator GetAnimator();
            }
        }
        public Conditions conditions;
    }
    public Profile[] profiles;

    public void FixedUpdate()
    {
        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i].conditions.Check(this))
            {
                movement.selectedSubroutine = profiles[i].movementSubroutine;
                movement.activeTarget = profiles[i].conditions.activeTarget;

                orientation.selectedSubroutine = profiles[i].orientationSubroutine;
                orientation.activeTarget = profiles[i].conditions.activeTarget;

                action.selectedSubroutine = profiles[i].actionSubroutine;
                action.activeTarget = profiles[i].conditions.activeTarget;
            }
        }
    }
}