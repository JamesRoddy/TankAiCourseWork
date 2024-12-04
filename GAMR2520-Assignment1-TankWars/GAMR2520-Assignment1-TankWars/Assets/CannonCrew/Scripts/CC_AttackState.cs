using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
public class CC_AttackState : BaseST
{
    private CC_SmartTank Tank;
    //Game object to store enemy position
    GameObject EnemyTankPositionStore;

    public CC_AttackState(CC_SmartTank newTank)
    {
        Tank = newTank;
    }
    public override Type Entry()
    {
        return null;
    }

    public override Type Update()
    {

        if (Tank.enemyTanksFound != null) // if we see the enemy tank
        {
            // store enemy position
            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;
            //fire at the stored position
            Tank.TurretFireAtPoint(EnemyTankPositionStore);
            // return null since the state doesn't change, we will continue attacking
            return null;
        }

        else if (!EnemyTankPositionStore.transform.position.Equals(Tank.enemyTank.transform.position) &&
            Tank.priorityManager.checkQueue(PriorityManager.queuePriority.SAFE, CC_SmartTank.PRIORITIES.HEALTH) ||
            Tank.priorityManager.checkQueue(PriorityManager.queuePriority.MINOR, CC_SmartTank.PRIORITIES.HEALTH) ||
            Tank.priorityManager.checkQueue(PriorityManager.queuePriority.MAJOR, CC_SmartTank.PRIORITIES.HEALTH))
        // else if the enemy position changed and our health is either in safe, minor or major priority

        {
            //store the enemies last position this might not be needed though so I'll ask later
            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;
            //and chase them
            return typeof(Chase);
        }
        //otherwise if our health is low, retreat
        else if (Tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL, CC_SmartTank.PRIORITIES.HEALTH))
        {
            return typeof(Retreat);
        }

        else
        {
            return typeof(SearchState);
        }
    }

    public override Type Exit()
    {
        return null;
    }
}