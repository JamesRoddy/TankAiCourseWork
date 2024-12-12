using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTFSMRBSSearchState : BaseST
{
    // Start is called before the first frame update
    private CC_smartTankFSMRBSBT Tank;
    private List<GameObject> pointsOfInterest;
    float explorationTimer;
    GameObject behind = new GameObject();
    bool hasFoundConsuamble = false;
    private Type stateToReturn = null;
    List<Vector3> priorityPositions = new List<Vector3>();
    List<Vector3> visited;
    GameObject priorityPosition = new GameObject();
    Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    //private float currentSpeed = 0.85f;
    float checkBehindWaitTime = 0.0f;
    int logCounter = 0;

    public override Type Entry()
    {
        //  currentSpeed = 0.85f;
        priorityPosition = new GameObject();
        stateToReturn = null;
        Debug.Log("Entered Search " + logCounter);
        Tank.resetTimersIntoSearch(); // reset timers coming in from search
        Tank.stats["searchState"] = true; // search state true

        logCounter++;
        return null;
    }

    public override Type Update()
    {


        foreach (var item in Tank.rules.GetRules) // iterates through the rules
        {


            if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
            {
                return item.CheckRule(Tank.stats); // return the state
            }
        }

        return null;




    }
    public override Type Exit()
    {
        Debug.Log("Search Exit " + logCounter);

        Tank.stats["searchState"] = false;
        hasFoundConsuamble = false;
        logCounter++;
        stateToReturn = null;
        priorityPositions.Clear();
        organisedConsumables.Clear();
        //  currentSpeed = 0.85f;
        explorationTimer = 0.0f;
        priorityPosition = new GameObject();

        return null;
    }
}
