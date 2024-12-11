using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.TestTools;
using static CC_SmartTank;
using static PriorityManager;
public class ChaseRBS : BaseST
{

    private CC_SmartTankRBS Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;
    float tankAttackMinThresh = 20.0f;

    float t = 0.0f;
    bool hasSeenBase = false;
    int logCounter = 0;
    public ChaseRBS(CC_SmartTankRBS newtank)
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

            //if our tank is between max and min units away from the enemy and we are good on fuel, we go into the kite state
            /*            if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < 60f
                           && Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > tankAttackMinThresh)
                        {
                            Debug.Log("switch attack: greater than min attack dist and smaller than max attack dist and not low fuel or ammo " + logCounter);
                            logCounter++;
                            return typeof(DodgeState);
                        }*/

            if (Tank.stats["lowFuel"] == true || Tank.stats["lowHealth"] == true)
            {
/*                Debug.Log(" switch retreat due to fuel priority " + logCounter);
*/                logCounter++;
                return null;
            }



            return null;
        }


       
        //If there are no enemy tnaks in our vision we check for enemy bases
        if (Tank.stats["enemyBaseSeen"] == true || hasSeenBase)
        {
            hasSeenBase = true;

            //Once we have seen the enemy the base we travel towards it.
/*            Debug.Log("Chasing Enemy Bases");
*/            if (Tank.stats["enemyBaseSeen"] == true)// ensure base doesnt slip out of vision
            {
                Tank.FollowPathToWorldPoint(Tank.enemyBase, fSpeed);
            }
            else
            {
                Tank.FollowPathToWorldPoint(Tank.EnemyBasePos, fSpeed);
            }

            //if our tank is between  min units away from the enemy base and we are good on ammo, we go into the attack state
            if (Tank.stats["ammoCritical"] == false)
            {
                if (Tank.stats["withinRange"]
                && Tank.stats["ammoCritical"] == false)
                {
/*                    Debug.Log("Base switch attack: greater than min attack dist and smaller than max attack dist and ammo " + logCounter);
*/                    logCounter++;
                    hasSeenBase = false;
                    return null;
                }
            }
          /*  else if (Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
*//*                Debug.Log("chase switch to search chasing base no ammo");
*//*                return typeof(SearchStateRBS);
            }*/


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
                return typeof(SearchStateRBS);
            }
            else if (Tank.stats["lostSight"])
            {
                Debug.Log("should chase with timer ");
                Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);
            }

        }

        return null;
      

    }
}
