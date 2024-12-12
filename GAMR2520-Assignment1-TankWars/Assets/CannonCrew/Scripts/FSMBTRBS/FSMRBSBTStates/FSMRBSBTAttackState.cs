using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTFSMRBSAttack : BaseST
{
    CC_smartTankFSMRBSBT Tank;
    public override Type Entry()
    {
        Tank.stats["attackState"] = true;


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
        Tank.stats["attackState"] = false;


        return null;
    }
}

