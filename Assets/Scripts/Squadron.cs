using System.Collections.Generic;
using UnityEngine;

public class Squadron
{
    public string squadronName = "";
    public int numMechs = 1;
    public int shipSTR = 0;
    public int shipINT = 0;
    public int shipDEX = 0;

    public Vector3 leaderDestination = Vector3.zero;

    private bool gangUp = false;
    private int formation = 0;

    private GameObject squadronLeader;
    private List<GameObject> members = new List<GameObject>();
    private Dictionary<GameObject, List<GameObject>> targets = new Dictionary<GameObject, List<GameObject>>();
    private Dictionary<GameObject, int> moving = new Dictionary<GameObject, int>();

    public void Init()
    {
        leaderDestination = Vector3.zero;
        if (shipINT >= 4)
        {
            //Debug.Log(squadronName + " activating formation flying");
            gangUp = true;
            formation = 1;
        }
        else
        {
            gangUp = false;
            formation = 0;
        }
    }

    public int Formation()
    {
        return formation;
    }

    public bool GangUp()
    {
        return gangUp;
    }

    public List<GameObject> GetMembers()
    {
        return members;
    }

    public GameObject Leader()
    {
        return squadronLeader;
    }

    public int AddMember(GameObject newMember)
    {
        if (members.Count == 0)
        {
            squadronLeader = newMember;
        }
        members.Add(newMember);
        targets[newMember] = null;
        moving[newMember] = 0;
        return members.Count - 1;
    }

    public void RemoveMember(GameObject oldMember)
    {
        moving.Remove(oldMember);
        targets.Remove(oldMember);
        bool pickNewLeader = false;
        if (squadronLeader == oldMember)
        {
            pickNewLeader = true;
        }
        members.Remove(oldMember);
        if (pickNewLeader)
        {
            if (members.Count > 0)
            {
                squadronLeader = members[Random.Range(0, members.Count)];
            }
            else
            {
                squadronLeader = null;
            }
        }
    }

    public List<GameObject> GetTargets()
    {
        List<GameObject> targetObjects = new List<GameObject>();
        if (gangUp)
        {
            if (targets.ContainsKey(squadronLeader))
            {
                if ((targets[squadronLeader] != null) && (targets[squadronLeader].Count > 0))
                {
                    targetObjects.Add(targets[squadronLeader][0]);
                }
            }
        }
        else
        {
            foreach (List<GameObject> targetSet in targets.Values)
            {
                if (targetSet != null)
                {
                    for (int i = 0; i < targetSet.Count; i++)
                    {
                        targetObjects.Add(targetSet[i]);
                    }
                }
            }
        }
        return targetObjects;
    }

    public void AddTargets(GameObject squadronMember, List<GameObject> newTargets)
    {
        targets[squadronMember] = newTargets;
    }

    // report moving, returns true if whole sqaudron in same or next state
    public bool ReportMovement(GameObject gameObject, int moveState)
    {
        bool moveStateMatches = true;

        if (formation == 1)
        {
            moving[gameObject] = moveState;
            foreach (int move in moving.Values)
            {
                if (!(move == moveState || move == (moveState + 1) % 3))
                {
                    moveStateMatches = false;
                    break;
                }
            }
        }
        return moveStateMatches;
    }

}
