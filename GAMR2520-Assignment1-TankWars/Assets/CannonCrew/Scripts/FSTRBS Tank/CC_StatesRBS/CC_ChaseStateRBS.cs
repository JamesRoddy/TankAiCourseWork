using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class ChaseRBS : BaseST
{

    private CC_SmartTankRBS Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;
    float tankAttackMinThresh = 10.0f;
    int logCounter = 0;
    public ChaseRBS(CC_SmartTankRBS newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        Tank.stats["chaseState"] = true;
        Debug.Log("Entered Chase " + logCounter);
        logCounter++;
        fSpeed = 1f;
        return null;
    }

    public override Type Exit()
    {
        Tank.stats["chaseState"] = true;
        Debug.Log("Chase Exit " + logCounter);
        logCounter++;

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

        if (Tank.enemyTank != null && Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            /*   EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank*/
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot

            //if our tank is between max and min units away from the enemy and we are good on fuel, we go into the attack state
            if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < Tank.TankFiringDistance
                && Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > tankAttackMinThresh
                && Tank.priorityManager.checkHigh(PRIORITIES.FUEL) && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
                Debug.Log("switch attack: greater than min attack dist and smaller than max attack dist and not low fuel or ammo " + logCounter);
                logCounter++;
                //return typeof(CC_AttackStateRBS);
            }


            return null;

        }

        else if (Tank.enemyTank == null && Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot

            if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < 5f)
            {
                Debug.Log("switch search tank no longer visible after moving to last known pos " + logCounter);
                logCounter++;
                return typeof(SearchStateRBS);
            }
            return null;
        }

        else if (Tank.enemyTank != null && Tank.priorityManager.checkLow(PRIORITIES.FUEL))
        {
            Debug.Log(" switch retreat due to fuel priority " + logCounter);
            logCounter++;
            return typeof(RetreatRBS);
        }

        else
        {
            Debug.Log("switch search no condtion met in chase " + logCounter);
            logCounter++;
            return typeof(SearchStateRBS);
        }

    }
}
