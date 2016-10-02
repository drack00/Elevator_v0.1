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

    public static bool GetApproval(AIControlled_MovingObject mo)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo))
                return group.CheckForApproval(mo);
        }

        return false;
    }
    public static bool GetApproval(AIControlled_MovingObject mo, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag)
                return group.CheckForApproval(mo);
        }

        return false;
    }
    public static bool GetApproval(AIControlled_MovingObject mo, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(mo);
        }

        return false;
    }
    public static bool GetApproval(AIControlled_MovingObject mo, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(mo);
        }

        return false;
    }

    public static void RemoveApproval (AIControlled_MovingObject mo)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo))
                group.RemoveFromApprovedMembers(mo);
        }
    }
    public static void RemoveApproval(AIControlled_MovingObject mo, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag)
                group.RemoveFromApprovedMembers(mo);
        }
    }
    public static void RemoveApproval(AIControlled_MovingObject mo, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(mo);
        }
    }
    public static void RemoveApproval(AIControlled_MovingObject mo, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(mo);
        }
    }

    [HideInInspector]
    public List<AIControlled_MovingObject> members;
    private List<AIControlled_MovingObject> approvedMembers;
    public int maxApprovals;

    void Awake()
    {
        members = new List<AIControlled_MovingObject>();
        approvedMembers = new List<AIControlled_MovingObject>();
    }

    public bool CheckForApproval (AIControlled_MovingObject mo)
    {
        if (approvedMembers.Contains(mo))
            return true;
        else if (approvedMembers.Count < maxApprovals)
        {
            approvedMembers.Add(mo);

            return true;
        }

        return false;
    }

    public void RemoveFromApprovedMembers(AIControlled_MovingObject mo)
    {
        if (approvedMembers.Contains(mo))
            approvedMembers.Remove(mo);
    }
}