using System;
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
    float chaseTime = 2.0f;
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

        Tank.SetEnemySeen();
        Tank.checkAmmo();
        Tank.CheckFuel();
        Tank.CheckHealth();
        Tank.IsWithinRange();
        Tank.CheckCanAttack();
        Tank.CheckShouldRetreat();
        Tank.CheckShouldChase();

        foreach (var item in Tank.rules.GetRules) // iterates through the rules
        {
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
                Debug.Log(" switch retreat due to fuel priority " + logCounter);
                logCounter++;
                return null;
            }
        }



        //If there are no enemy tnaks in our vision we check for enemy bases
        if (Tank.stats["enemyBaseSeen"] == true || hasSeenBase)
        {
            hasSeenBase = true;

            //Once we have seen the enemy the base we travel towards it.
            Debug.Log("Chasing Enemy Bases");
            if (Tank.stats["enemyBaseSeen"] == true)// ensure base doesnt slip out of vision
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
                    Debug.Log("Base switch attack: greater than min attack dist and smaller than max attack dist and ammo " + logCounter);
                    logCounter++;
                    hasSeenBase = false;
                    return null;
                }
            }
            else if (Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
                Debug.Log("chase switch to search chasing base no ammo");
                return null;
            }


            return null;




        }
        else if (t < chaseTime && Tank.enemyTank == null && Tank.enemyBase == null)
        {
            Debug.Log("Chasing with timer ");
            t += Time.deltaTime;
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot




            return null;
        }

        //Chase the enemy tank once it gets outside of our range
        if (Tank.enemyTank == null)
        {
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot

            if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < 5f)
            {
                //Questionable change
                if (Tank.enemyTank != null)
                {
                    Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);
                    Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);

                    if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < Tank.TankFiringDistance
                        && Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > tankAttackMinThresh)
                    {
                        Debug.Log("Switches to attack after it tries to chase a retreating tank");
                        return null;
                    }

                    else
                    {
                        return null;
                    }

                }

                Debug.Log("switch search tank no longer visible after moving to last known pos " + logCounter);
                logCounter++;
                return null;
            }
            return null;
        }

        return null;

    }
}
