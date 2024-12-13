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


        if (Tank.chasingEnemy.evaluate() == BTNODESTATES.SUCCESS) // if we succeded the attack sequence check all of the rules to see the next state transition 
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


        Debug.Log("Chase Exit " + logCounter);
        logCounter++;

        Tank.stats["chaseState"] = false;

        t = 0.0f;
        fSpeed = 1f;
        return null;
    }




}
