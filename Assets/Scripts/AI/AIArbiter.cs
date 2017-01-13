using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIArbiter : MonoBehaviour
{
    [System.Serializable]
    public class Profile
    {
        public int maxMembers;
        public float maxDistance;

        public int maxGroups;
        private List<AIGroup> groups = new List<AIGroup>();

        [System.Serializable]
        public struct Conditions
        {
            public string name;
            public string tag;
            public LayerMask layerMask;

            public bool Check(AI_Master ai)
            {
                bool _check =
                    (string.IsNullOrEmpty(name) || ai.name == name) &&
                    (string.IsNullOrEmpty(tag) || ai.tag == tag) &&
                    (layerMask == 0 || (LayerMask.GetMask(LayerMask.LayerToName(layerMask)) & ai.gameObject.layer) != 0);

                return _check;
            }
        }
        public Conditions conditions;

        public void Update()
        { 
            //iterate through all moving objects
            foreach (AI_Master ai in GameObject.FindObjectsOfType<AI_Master>())
            {
                //if moving object meets conditions for this profile
                if (conditions.Check(ai))
                {
                    //check if a group has been set
                    bool setGroup = false;

                    //iterate through all groups in this profile
                    foreach (AIGroup group in groups)
                    {
                        //if this group already has the moving object as a member
                        if (group.members.Contains(ai))
                        {
                            //break the group assignment loop
                            setGroup = true;
                            break;
                        }

                        //if an existing group can accept this moving object as an additional member
                        if ((group.members.Count < maxMembers || maxMembers < 0) && (Vector3.Distance(ai.transform.position, group.transform.position) < maxDistance || maxDistance < 0.0f))
                        {
                            //add this moving object to the group
                            group.members.Add(ai);

                            //break the group assignment loop
                            setGroup = true;
                            break;
                        }
                    }

                    //if no groups within this profile can/have accept(ed) this moving object as a member, and this profile can have more attached groups
                    if (!setGroup && (groups.Count < maxGroups || maxGroups < 0))
                    {
                        //create a new group, add the moving object to the group, and add the group to the profile
                        AIGroup group = new GameObject("AIGroup", typeof(AIGroup)).GetComponent<AIGroup>();
                        group.members.Add(ai);
                        groups.Add(group);
                    }
                }
                //otherwise
                else
                {
                    //iterate through all groups in this profile
                    foreach (AIGroup group in groups)
                    {
                        //if this group contains the moving object
                        if(group.members.Contains(ai))
                        {
                            //remove it
                            group.members.Remove(ai);
                        }
                    }
                }
            }
        }
    }
    public Profile[] profiles;

    public void FixedUpdate()
    {
        foreach(Profile profile in profiles)
        {
            profile.Update();
        }
    }
}
