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
    public CC_AttackState(CC_SmartTank newTank)
    {
        Tank = newTank;
    }
    public override Type Entry()
    {
        Debug.Log("Attack Enter " + logCounter);
        logCounter++;
        return null;
    }

    public override Type Update()
    {

        if (Tank.enemyTank != null) // if we see the enemy tank
        {
            // store enemy position
            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;
            //fire at the stored position
            Tank.TurretFireAtPoint(Tank.LastKnownEPos);
            // return null since the state doesn't change, we will continue attacking

            if (Tank.priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.HEALTH))
            {
                Debug.Log("attack switch to retreat low health " + logCounter);
                logCounter++;
                return typeof(Retreat);
            }

            else
            {
                return null;
            }
        }

        else if ( Tank.priorityManager.checkHigh(PRIORITIES.HEALTH)    
                 || (Tank.priorityManager.checkQueue(queuePriority.MAJOR,PRIORITIES.HEALTH) 
                 && Tank.priorityManager.isResourceSafe(PRIORITIES.FUEL )))
            // else if the enemy position changed and our health is either in safe, minor or major priority

        {

            //store the enemies last position this might not be needed though so I'll ask later
            //EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;
            //and chase them
            if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > Tank.TankFiringDistance &&!Tank.priorityManager.checkQueue(queuePriority.CRITICAL,PRIORITIES.AMMO))
            {
                Debug.Log("attack switch to chase tank out of firing range " + logCounter);
                logCounter++;
                return typeof(Chase);

            }
            else
            {
                Debug.Log("attack switch to search low on ammo " + logCounter);
                logCounter++;
                return typeof(SearchState);
            }
          
          
        }
        //otherwise if our health is low, retreat
        else if (Tank.priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.HEALTH))
        {
            Debug.Log("attack switch to retreat health low " + logCounter);

            return typeof(Retreat);
        }
        
        else 
        {
            Debug.Log("attack switch to search no condtion was hit " + logCounter);

            return typeof(SearchState);
        }
    }

    public override Type Exit()
    {
        Debug.Log("Attack Exit "+ logCounter);
        logCounter++;
        return null;
    }
}