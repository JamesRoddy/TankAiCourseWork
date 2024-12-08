using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class CC_AttackStateRBS : BaseST
{
    private CC_SmartTankRBS Tank;
    //Game object to store enemy position
    GameObject EnemyTankPositionStore = new GameObject();
    int logCounter = 0;
    float fshootTimeLimit = 2f;
    float fkiteTime = 10f;
    float baseDeadTimer = 2.20f;
    float t;
    float fSpeed = 0.8f;
    float fshootT;
    bool isFiringAtBase = false;
    public CC_AttackStateRBS(CC_SmartTankRBS newTank)
    {
        Tank = newTank;
    }
    public override Type Entry()
    {
        Debug.Log("Attack Enter " + logCounter);
        logCounter++;

        Tank.stats["attackState"] = true;

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
        Tank.SetEnemyBaseSeen();
        Tank.CheckSpeed();
        Tank.AttackEnemyBase();
        Tank.enemyBaseWithinRange();

        foreach (var item in Tank.rules.GetRules) // iterates through the rules
        {
            if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
            {
                return item.CheckRule(Tank.stats); // return the state
            }
        }


        if (Tank.enemyTank != null) // if we see the enemy tank
        {

            if (Tank.priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.HEALTH))
            {
                // Debug.Log("attack switch to retreat low health " + logCounter);
                logCounter++;
                return null;
            }

            if (Tank.priorityManager.checkHigh(PRIORITIES.FUEL) && Tank.priorityManager.checkHigh(PRIORITIES.HEALTH))
            {
                Debug.Log("priorities hit to chase ");
                if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > Tank.TankFiringDistance
                   && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
                {
                    Debug.Log("attack switch to chase tank out of firing range " + logCounter);
                    logCounter++;
                    return typeof(ChaseRBS);

                }
            }


            //fire at the stored position
            Tank.TurretFireAtPoint(Tank.LastKnownEPos);
            // return null since the state doesn't change, we will continue attacking
            return null;


        }


        if (Tank.enemyBase != null && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
        {

            t += Time.deltaTime;
            if (baseDeadTimer < t)
            {
                Debug.Log("base dead");
                isFiringAtBase = false;
                t = 0.0f;
            }
            //Potential to do 
            /*   GameObject inverseEnemeyBase = new GameObject();
               inverseEnemeyBase.transform.position = new Vector3(Tank.transform.forward.x , 0, Tank.transform.position.z + -Tank.transform.forward.z*5.0f);
               */
            /*if (Tank.stopAndCheckPos(inverseEnemeyBase, 2.5f,Tank.enemyTank)) {*/

            /* if(Tank.enemyTank != null)
             {
                 Debug.Log("saw enemy tank before attacking base ");
                 return null;
             }*/
            Debug.Log("Attacking enemy base");
            if (isFiringAtBase != true)
            {
                Tank.TurretFireAtPoint(Tank.EnemyBasePos);
                isFiringAtBase = true;
            }



            Debug.Log("go into search after firing at base preventing chase with timer bug");

            return null; ;
        }

        Debug.Log("is enemy base null");
        Debug.Log("attack switch to search no condtion was hit " + logCounter);
        return typeof(SearchStateRBS);


        /*  Debug.Log("attack switch to search low on res " + logCounter);
          logCounter++;
          return typeof(SearchState);*/


    }

    public override Type Exit()
    {
        /*if(bCalc == false)
        {
            kitePath.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * fkiteRadius, 0.0f, Mathf.Cos(Time.realtimeSinceStartup) * fkiteRadius);
            kiteTankPosition.transform.position = Tank.LastKnownEPos.transform.position + kitePath.transform.position;
            bCalc = true;
        }*/
        Debug.Log("Attack Exit " + logCounter);
        fshootT = 0f;
        isFiringAtBase = false;
        t = 0.0f;
        logCounter++;

        Tank.stats["attackState"] = false;
        return null;
    }
}