using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AI_Movement))]
[RequireComponent(typeof(AI_Orientation))]
[RequireComponent(typeof(AI_Action))]
public class AI_Master : MonoBehaviour
{
    public MovingObject movingObject
    {
        get
        {
            return GetComponent<MovingObject > ();
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
                            if ((LayerMask.GetMask(LayerMask.LayerToName(go.layer)) & targetsLayerMask) != 0)
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

            public bool belowHealthThreshold;
            public float healthThreshold;
            public bool belowStunThreshold;
            public float stunThreshold;

            public bool requireGroupApproval;
            public string groupTag;
            public LayerMask groupLayerMask;

            private float _warmup;
            public float warmup;
            private float _offDelay;
            public float offDelay;
            private float _cooldown;
            public float cooldown;

            public bool Check(AI_Master master)
            {
                MovingObject mo = master.movingObject;

                //stat check
                bool _check = haveTarget &&
                    belowHealthThreshold == (mo.health < healthThreshold) &&
                    belowStunThreshold == (mo.stun < stunThreshold);

                //group approval
                if (requireGroupApproval)
                {
                    if (_check)
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
                    else
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
                }

                //warmup frames
                if (_check && Mathf.Approximately(_cooldown, Mathf.Epsilon))
                {
                    _warmup += Time.deltaTime;
                    if (_warmup > warmup)
                        _warmup = warmup;
                }
                else
                {
                    _warmup = 0.0f;
                }
                _check = _check && Mathf.Approximately(_warmup, warmup);

                //off delay frames
                if (_check)
                {
                    if (Mathf.Approximately(_offDelay, Mathf.Epsilon))
                        _offDelay = offDelay;
                }
                else
                {
                    _offDelay -= Time.deltaTime;
                    if (_offDelay < 0.0f)
                        _offDelay = 0.0f;
                }
                _check = _check || !Mathf.Approximately(_offDelay, Mathf.Epsilon);

                //cooldown frames
                if (_cooldown > 0.0f)
                {
                    _cooldown -= Time.deltaTime;
                    if (_cooldown < 0.0f)
                        _cooldown = 0.0f;
                }
                _check = _check && Mathf.Approximately(_cooldown, Mathf.Epsilon);
                if (_check)
                    _cooldown = cooldown;

                return _check;
            }
        }
        public Conditions conditions;
    }
    public Profile[] profiles;

    public void Update()
    {
        //assign profiles to slave AIs if conditions are met, if profile is masked pass to next highest priority profile, break when all slave profiles are set, reset AIs if a relevent profile cannot be found
        bool movementMasked = true;
        bool orientationMasked = true;
        bool actionMasked = true;
        for (int i = 0; i < profiles.Length; i++)
        {
            if (profiles[i].conditions.Check(this))
            {
                if (movementMasked && profiles[i].movementSubroutine != AI_Movement.Subroutine.Masked)
                {
                    movement.selectedSubroutine = profiles[i].movementSubroutine;
                    movement.activeTarget = profiles[i].conditions.activeTarget;
                    movementMasked = false;
                }
                if (orientationMasked && profiles[i].orientationSubroutine != AI_Orientation.Subroutine.Masked)
                {
                    orientation.selectedSubroutine = profiles[i].orientationSubroutine;
                    orientation.activeTarget = profiles[i].conditions.activeTarget;
                    orientationMasked = false;
                }
                if (actionMasked && profiles[i].actionSubroutine != AI_Action.Subroutine.Masked)
                {
                    action.selectedSubroutine = profiles[i].actionSubroutine;
                    action.activeTarget = profiles[i].conditions.activeTarget;
                    actionMasked = false;
                }
            }

            if (!movementMasked && !orientationMasked && !actionMasked)
                break;

            if(i == profiles.Length - 1)
            {
                if (movementMasked)
                    movement.Reset();
                if (orientationMasked)
                    orientation.Reset();
                if (actionMasked)
                    action.Reset();
            }
        }
    }
}