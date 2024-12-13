using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTFSMRBSSearchState : BaseST
{
    // Start is called before the first frame update
    private CC_smartTankFSMRBSBT Tank;

    float explorationTimer;
    GameObject behind = new GameObject();
    bool hasFoundConsuamble = false;
    private Type stateToReturn = null;
    List<Vector3> priorityPositions = new List<Vector3>();
    
    Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    //private float currentSpeed = 0.85f;
    float checkBehindWaitTime = 0.0f;
    int logCounter = 0;

    public CC_BTFSMRBSSearchState(CC_smartTankFSMRBSBT tank){
        
       Tank = tank;
    }

    public override Type Entry()
    {
        //  currentSpeed = 0.85f;
      
        stateToReturn = null;
        Tank.stats["searchState"] = true; // search state true

        logCounter++;
        return null;
    }

    public override Type Update()
    {
        if ((!Tank.evaluateSequences(Tank.sequencesFromSearch) && !Tank.evaluateSelectors(Tank.selectorsFromSearch))
              && Tank.checkSearch.evaluate() == BTNODESTATES.SUCCESS
            
            ) // if we dont need to execute any other sequences or selectors that would require us to switch from search) // we will continue searching 
        {

            foreach (var item in Tank.rules.GetRules) // iterates through the rules
            {

                if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
                {
                    return item.CheckRule(Tank.stats); // return the state
                }
            }

        }

        return null;




    }
    public override Type Exit()
    {

        Tank.stats["searchState"] = false;
        Debug.Log(Tank.stats["searchState"]);
        hasFoundConsuamble = false;
        logCounter++;
        stateToReturn = null;
        priorityPositions.Clear();
        organisedConsumables.Clear();
        //  currentSpeed = 0.85f;
        explorationTimer = 0.0f;

        return null;
    }
}
