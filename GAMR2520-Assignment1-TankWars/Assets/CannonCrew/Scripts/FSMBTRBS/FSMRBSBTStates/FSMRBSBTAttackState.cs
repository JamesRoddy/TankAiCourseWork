using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTFSMRBSAttack : BaseST
{
    CC_smartTankFSMRBSBT Tank;

    public BTFSMRBSAttack(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
    }

    public override Type Entry()
    {
        Tank.stats["attackState"] = true;

        Debug.Log("Entered Attack");
        return null;
    }
    public override Type Update()
    {

        Debug.Log("attack enemy "+Tank.attackingEnemy.evaluate());
        if(Tank.attackEnemy.evaluate() == BTNODESTATES.SUCCESS && Tank.attackingBase.evaluate() == BTNODESTATES.SUCCESS) // if we succeded the attack sequence check all of the rules to see the next state transition 
        {
            foreach (var item in Tank.rules.GetRules) // iterates through the rules
            {
                if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
                {
                    return item.CheckRule(Tank.stats); // return the state
                }
            }
            // again using the simiplicty of the state machine the serach state can be the default state without any extra rules attacked expect the search state its self 

            return typeof(CC_BTFSMRBSSearchState);
        }

        return null;




    }

    public override Type Exit()
    {
        Tank.stats["attackState"] = false;

        Debug.Log("Exited Attack");
        return null;
    }
}

