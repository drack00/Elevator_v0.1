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

    public static bool GetApproval(AI_Master ai)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(ai))
                return group.CheckForApproval(ai);
        }

        return false;
    }
    public static void RemoveApproval (AI_Master ai)
    {
        foreach (AIGroup group in allGroups)
        {
            if (group.members.Contains(ai))
                group.RemoveFromApprovedMembers(ai);
        }
    }

    public bool CheckForApproval(AI_Master ai)
    {
        if (approvedMembers.Contains(ai))
            return true;
        else if (approvedMembers.Count < 1)
        {
            approvedMembers.Add(ai);

            return true;
        }

        return false;
    }
    public void RemoveFromApprovedMembers(AI_Master ai)
    {
        if (approvedMembers.Contains(ai))
            approvedMembers.Remove(ai);
    }

    [HideInInspector]
    public List<AI_Master> members = new List<AI_Master>();
    private List<AI_Master> approvedMembers = new List<AI_Master>();

    public void FixedUpdate()
    {
        if (members.Count < 1)
            Destroy(gameObject);
        else
            transform.position = members[0].transform.position;
    }
}