using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIGroup : MonoBehaviour {
    public static AIGroup[] allGroups
    {
        get
        {
            return FindObjectsOfType<AIGroup>();
        }
    }

    public static bool GetApproval(AI_Master.Profile.Conditions.ICanCheck canCheck)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck))
                return group.CheckForApproval(canCheck);
        }

        return false;
    }
    public static bool GetApproval(AI_Master.Profile.Conditions.ICanCheck canCheck, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag)
                return group.CheckForApproval(canCheck);
        }

        return false;
    }
    public static bool GetApproval(AI_Master.Profile.Conditions.ICanCheck canCheck, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(canCheck);
        }

        return false;
    }
    public static bool GetApproval(AI_Master.Profile.Conditions.ICanCheck canCheck, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(canCheck);
        }

        return false;
    }

    public static void RemoveApproval (AI_Master.Profile.Conditions.ICanCheck canCheck)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck))
                group.RemoveFromApprovedMembers(canCheck);
        }
    }
    public static void RemoveApproval(AI_Master.Profile.Conditions.ICanCheck canCheck, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag)
                group.RemoveFromApprovedMembers(canCheck);
        }
    }
    public static void RemoveApproval(AI_Master.Profile.Conditions.ICanCheck canCheck, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(canCheck);
        }
    }
    public static void RemoveApproval(AI_Master.Profile.Conditions.ICanCheck canCheck, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(canCheck);
        }
    }

    [HideInInspector]
    public List<AI_Master.Profile.Conditions.ICanCheck> members;
    private List<AI_Master.Profile.Conditions.ICanCheck> approvedMembers;
    public int maxApprovals;

    void Awake()
    {
        members = new List<AI_Master.Profile.Conditions.ICanCheck>();
        approvedMembers = new List<AI_Master.Profile.Conditions.ICanCheck>();
    }

    public bool CheckForApproval (AI_Master.Profile.Conditions.ICanCheck canCheck)
    {
        if (approvedMembers.Contains(canCheck))
            return true;
        else if (approvedMembers.Count < maxApprovals)
        {
            approvedMembers.Add(canCheck);

            return true;
        }

        return false;
    }

    public void RemoveFromApprovedMembers(AI_Master.Profile.Conditions.ICanCheck canCheck)
    {
        if (approvedMembers.Contains(canCheck))
            approvedMembers.Remove(canCheck);
    }
}