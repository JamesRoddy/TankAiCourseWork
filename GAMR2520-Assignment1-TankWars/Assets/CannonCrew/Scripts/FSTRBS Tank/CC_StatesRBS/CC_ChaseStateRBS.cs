using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.TestTools;
using static CC_SmartTank;
using static PriorityManager;
public class CC_ChaseStateRBS : BaseST
{

    private CC_SmartTankRBS Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;
    float tankAttackMinThresh = 20.0f;

    float t = 0.0f;
    int logCounter = 0;
    public CC_ChaseStateRBS(CC_SmartTankRBS newtank)
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

    public override Type Exit()
    {
        

        Debug.Log("Chase Exit " + logCounter);
        logCounter++;

        Tank.stats["chaseState"] = false;
     
        t = 0.0f;
        fSpeed = 1f;
        return null;
    }

    public override Type Update()
    {

        foreach (var item in Tank.rules.GetRules) // iterates through the rules
        {
            Debug.Log("current rule being checked is IN CHASE STATE" + item.debugType.GetType());
            Debug.Log(item.debugType.GetType() + "antecedent a is " + item.antecentA + " is " + Tank.stats[item.antecentA] + " antecedent b is " + item.antecentB + " is " + Tank.stats[item.antecentB]);
            if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
            {
                Debug.Log("rule fired " + item.debugType.GetType());
            }

            if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
            {
                return item.CheckRule(Tank.stats); // return the state
            }
        }
        //First we check for any enemy tanks in our vision
        if (Tank.stats["enemySeen"] == true && Tank.stats["highFuel"] == true)
        {
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot

            return null;
        }


       
        //If there are no enemy tnaks in our vision we check for enemy bases
        if (Tank.stats["enemyBaseSeen"] == true )
        {
            
            Debug.Log(Tank.stats["enemyBaseSeen"] + " enemy base seen is ");
            //Once we have seen the enemy the base we travel towards it.
            if (Tank.stats["enemyBaseSeen"] == true)// ensure base doesnt slip out of vision
            {
                Tank.FollowPathToWorldPoint(Tank.enemyBase, fSpeed);
            }
           
            return null;

        }
        // every state will have a default return state allowing us to focus on more complex sides of the state governed by the global rules while still being able to use 
        // the simplicity of the finite state machine 
      
        if (!Tank.stats["shouldChase"])
        {
     
            
            if (!Tank.stats["lostSight"])
            {
                Debug.Log("chase timer is false and lost sight is false ");
                Debug.Log("should going into SEARCH from CHASE with TIMER    ");
                return typeof(CC_SearchStateRBS);
            }
            else if (Tank.stats["lostSight"] )
            {
                Debug.Log("should chase with timer ");
                Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);
            }

        }

        return null;
      

    }




}
