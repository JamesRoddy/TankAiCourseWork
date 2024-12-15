using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class CC_AttackState : BaseST
{
    private CC_SmartTank Tank;
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
    public CC_AttackState(CC_SmartTank newTank)
    {
        Tank = newTank;
    }
    public override Type Entry()
    {
        logCounter++;
        return null;
    }

    public override Type Update()
    {

        if (Tank.enemyTank != null) // if we see the enemy tank
        {
           
            if (Tank.priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.HEALTH))
            {
                logCounter++;
                return typeof(CC_Retreat);
            }

            if (Tank.priorityManager.checkHigh(PRIORITIES.FUEL) && Tank.priorityManager.checkHigh(PRIORITIES.HEALTH))
            {
                if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > Tank.TankFiringDistance
                   && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
                {
                    logCounter++;
                    return typeof(CC_Chase);

                }
            }

           
            //fire at the stored position
            Tank.TurretFireAtPoint(Tank.LastKnownEPos);
            // return null since the state doesn't change, we will continue attacking
            return null;


        }


        if (Tank.enemyBase != null  && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
        {

            t += Time.deltaTime;
            if(baseDeadTimer<t  )
            {
                isFiringAtBase = false;
                t = 0.0f;
            }

            if(isFiringAtBase != true)
            {
                Tank.TurretFireAtPoint(Tank.EnemyBasePos);
                isFiringAtBase = true; 
            }

            return null;
        }

         return typeof(CC_SearchState);
    }

    public override Type Exit()
    {
        fshootT = 0f;
        isFiringAtBase = false;
        t = 0.0f;
        logCounter++;
        return null;
    }
}