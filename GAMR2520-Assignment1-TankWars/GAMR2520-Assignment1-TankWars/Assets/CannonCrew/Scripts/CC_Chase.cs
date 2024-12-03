using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Chase : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore;
    float fSpeed;

    public Chase(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        fSpeed = 1f;
        return null;
    }

    public override Type Exit()
    {
        fSpeed = 1f;
        return null;
    }

    public override Type Update()
    {
        if (Tank != null)
        {
            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
            Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(EnemyTankPositionStore, fSpeed);  //Follow the tank so that we have a more accurate shot

            //if our tank is less than 25 units away from the enemy and we are good on fuel, we go into the attack state
            if (Vector3.Distance(Tank.transform.position, EnemyTankPositionStore.transform.position) < 25f 
                && (Tank.priorityManager.checkQueue(PriorityManager.queuePriority.SAFE, CC_SmartTank.PRIORITIES.FUEL) || 
                Tank.priorityManager.checkQueue(PriorityManager.queuePriority.MINOR, CC_SmartTank.PRIORITIES.FUEL)))
            {
                return typeof(CC_AttackState);
            }

            else
            {
                return null;
            }
        }

        else
        {
            return typeof(SearchState);
        }

    }
}
