﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HitBox : ActiveFrameData
{
    public LayerMask grabLayers;
    public Vector3 grabOffset;
    public LayerMask targetLayers;
    public bool continuous;
    public int maxHurts;
    public bool retainHurts;
    public LayerMask clashLayers;
    public string clashName = "Clash";

    [System.Serializable]
	public class HitBehaviour : Behaviour { }
	public HitBehaviour hitBehaviour;

	[System.Serializable]
	public class ResponseBehaviour : Behaviour
    {
        public ApplyTimeScale timeScale;

        public void Do(bool continous, HitBox hit, HurtBox hurt)
        {
            timeScale.Do(hurt.gameObject);

            base.Do(continous, hit, hurt, true);
        }
    }
	public ResponseBehaviour responseBehaviour;

    private List<HurtBox> hurts;
    [HideInInspector]
    public List<MovingObject> exclude;
    public override void Awake ()
    {
        base.Awake();

        hurts = new List<HurtBox> ();
        exclude = new List<MovingObject>();
	}
	void OnEnable ()
    {
        hurts = new List<HurtBox> ();
        exclude = new List<MovingObject>();
    }
	void OnDisable ()
    {
        mo.StopGrabbing();

        hurts = new List<HurtBox> ();
        exclude = new List<MovingObject>();
    }
		
	void OnTriggerEnter (Collider other)
    {
        if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & clashLayers) != 0)
        {
            mo.Clash(clashName);

            return;
        }

        if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & grabLayers) != 0)
        {
            mo.StartGrabbing(other.attachedRigidbody.GetComponent<MovingObject>(), grabOffset);

            return;
        }

        if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0 ||
            other.GetComponent<HurtBox>() == null ||
            hurts.Contains(other.GetComponent<HurtBox>()) ||
            exclude.Contains(other.GetComponent<HurtBox>().mo))
            return;
        
        HurtBox hurt = other.GetComponent<HurtBox>();
		
		if (hurts.Count < maxHurts || maxHurts == -1)
        {
            hurts.Add (hurt);

            if (!continuous)
            {
                hitBehaviour.Do(false, this, hurt);
                responseBehaviour.Do(false, this, hurt, true);
            }
        }
	}

	void OnTriggerStay (Collider other)
    {
        if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & clashLayers) != 0)
        {
            mo.Clash(clashName);

            return;
        }

        if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0 || 
            other.GetComponent<HurtBox>() == null || 
            !hurts.Contains(other.GetComponent<HurtBox>()) ||
            exclude.Contains(other.GetComponent<HurtBox>().mo))
			return;

        HurtBox hurt = other.GetComponent<HurtBox>();

		bool otherValid = false;
		foreach (HurtBox _hurt in hurts)
        {
			if (hurt == _hurt)
            {
				otherValid = true;
				break;
			}
		}

		if (otherValid && continuous)
        {
            hitBehaviour.Do(true, this, hurt);
            responseBehaviour.Do(true, this, hurt, true);
        }
	}

	void OnTriggerExit (Collider other)
    {
		if ((LayerMask.GetMask(LayerMask.LayerToName(other.gameObject.layer)) & targetLayers) == 0 || 
            other.GetComponent<HurtBox>() == null || 
            !hurts.Contains(other.GetComponent<HurtBox>()) ||
            exclude.Contains(other.GetComponent<HurtBox>().mo))
			return;

        HurtBox hurt = other.GetComponent<HurtBox>();

        TimeScaleManager.singleton.ResetTimeScale(other.gameObject);

        if (!retainHurts)
            hurts.Remove(hurt);
	}
}