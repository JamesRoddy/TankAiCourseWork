using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_BTFSMRBChaseState : BaseST
{


    private CC_smartTankFSMRBSBT Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;
    float tankAttackMinThresh = 20.0f;

    float t = 0.0f;
    bool hasSeenBase = false;
    int logCounter = 0;
    public CC_BTFSMRBChaseState(CC_smartTankFSMRBSBT newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Chase " + logCounter);
        logCounter++;

        Tank.stats["chaseState"] = true;

        fSpeed = 1f;
        t = 0.0f;
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


        Debug.Log("Chase Exit " + logCounter);
        logCounter++;

        Tank.stats["chaseState"] = false;

        t = 0.0f;
        fSpeed = 1f;
        return null;
    }




}
