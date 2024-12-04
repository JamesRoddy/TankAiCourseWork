using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class Chase : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;

    public Chase(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Chase");
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
        Debug.Log("In chase state");
        if (Tank.enemyTank != null && Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
            Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(EnemyTankPositionStore, fSpeed);  //Follow the tank so that we have a more accurate shot

            //if our tank is less than 25 units away from the enemy and we are good on fuel, we go into the attack state
            if (Vector3.Distance(Tank.transform.position, EnemyTankPositionStore.transform.position) < 40f 
                && Vector3.Distance(Tank.transform.position, EnemyTankPositionStore.transform.position) > 10f
                && Tank.priorityManager.checkHigh(PRIORITIES.FUEL) && Tank.TankCurrentAmmo != 0)
            {
                return typeof(CC_AttackState);
            }


            return null;

        }

        else if(Tank.enemyTank == null && Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(EnemyTankPositionStore, fSpeed);  //Follow the tank so that we have a more accurate shot

            if(Vector3.Distance(Tank.transform.position, EnemyTankPositionStore.transform.position) < 5f)
            {
                return typeof(SearchState);
            }
            return null;
        }

        else if (Tank.enemyTank != null && Tank.priorityManager.checkLow(PRIORITIES.FUEL))
        {
            return typeof(Retreat);
        }

        else
        { 
            return typeof(SearchState);
        }

    }
}
