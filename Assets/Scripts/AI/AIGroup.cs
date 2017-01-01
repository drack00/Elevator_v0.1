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

    public static bool GetApproval(MovingObject mo)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo))
                return group.CheckForApproval(mo);
        }

        return false;
    }
    public static bool GetApproval(MovingObject mo, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag)
                return group.CheckForApproval(mo);
        }

        return false;
    }
    public static bool GetApproval(MovingObject mo, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(mo);
        }

        return false;
    }
    public static bool GetApproval(MovingObject mo, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                return group.CheckForApproval(mo);
        }

        return false;
    }

    public static void RemoveApproval (MovingObject mo)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo))
                group.RemoveFromApprovedMembers(mo);
        }
    }
    public static void RemoveApproval(MovingObject mo, string tag)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag)
                group.RemoveFromApprovedMembers(mo);
        }
    }
    public static void RemoveApproval(MovingObject mo, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(mo);
        }
    }
    public static void RemoveApproval(MovingObject mo, string tag, LayerMask layerMask)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(mo) && group.tag == tag && (group.gameObject.layer & layerMask) != 0)
                group.RemoveFromApprovedMembers(mo);
        }
    }

    [HideInInspector]
    public List<MovingObject> members;
    private List<MovingObject> approvedMembers;
    public int maxApprovals;

    void Awake()
    {
        members = new List<MovingObject>();
        approvedMembers = new List<MovingObject>();
    }

    public bool CheckForApproval (MovingObject mo)
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

    public void RemoveFromApprovedMembers(MovingObject mo)
    {
        if (approvedMembers.Contains(mo))
            approvedMembers.Remove(mo);
    }
}