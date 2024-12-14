using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_BTFSMRBSRetreatState : BaseST
{
    CC_smartTankFSMRBSBT Tank;


    public CC_BTFSMRBSRetreatState(CC_smartTankFSMRBSBT tank)
    {

        Tank = tank;
    }

    public override Type Entry()
    {
        Tank.stats["retreatState"] = true;


        return null;
    }
    public override Type Update()
    {
        Debug.Log("RETREAT STATE BTFSMRBS");

        if (Tank.retreating.evaluate() == BTNODESTATES.SUCCESS) // if we succeded the attack sequence check all of the rules to see the next state transition 
        {
            foreach (var item in Tank.rules.GetRules) // iterates through the rules
            {
                if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
                {
                    return item.CheckRule(Tank.stats); // return the state
                }
            }
            return typeof(CC_BTFSMRBSSearchState);
        }
        return null;
    }

    public override Type Exit()
    {
        Tank.stats["retreatState"] = false;


        return null;
    }


}
