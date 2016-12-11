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

    public static bool GetApproval(AI.ICanCheck canCheck)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck))
                return group.CheckForApproval(canCheck);
        }

        return false;
    }
    public static bool GetApproval(AI.ICanCheck canCheck, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag)
                return group.CheckForApproval(canCheck);
        }

        return false;
    }
    public static bool GetApproval(AI.ICanCheck canCheck, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(canCheck);
        }

        return false;
    }
    public static bool GetApproval(AI.ICanCheck canCheck, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(canCheck);
        }

        return false;
    }

    public static void RemoveApproval (AI.ICanCheck canCheck)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck))
                group.RemoveFromApprovedMembers(canCheck);
        }
    }
    public static void RemoveApproval(AI.ICanCheck canCheck, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag)
                group.RemoveFromApprovedMembers(canCheck);
        }
    }
    public static void RemoveApproval(AI.ICanCheck canCheck, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(canCheck);
        }
    }
    public static void RemoveApproval(AI.ICanCheck canCheck, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(canCheck) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(canCheck);
        }
    }

    [HideInInspector]
    public List<AI.ICanCheck> members;
    private List<AI.ICanCheck> approvedMembers;
    public int maxApprovals;

    void Awake()
    {
        members = new List<AI.ICanCheck>();
        approvedMembers = new List<AI.ICanCheck>();
    }

    public bool CheckForApproval (AI.ICanCheck canCheck)
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

    public void RemoveFromApprovedMembers(AI.ICanCheck canCheck)
    {
        if (approvedMembers.Contains(canCheck))
            approvedMembers.Remove(canCheck);
    }
}