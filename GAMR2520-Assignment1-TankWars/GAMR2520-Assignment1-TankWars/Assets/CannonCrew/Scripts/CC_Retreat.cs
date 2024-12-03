using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retreat : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore;

    public Retreat(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        return null;
    }

    public override Type Exit()
    {
        return null;
    }

    public override Type Update()
    {
        if(Tank.CheckLowFuel() == false || Tank.CheckLowHealth() == false || Tank.CheckLowAmmo() == false)
        {
            return typeof(SearchState);
        }

        else
        {
            if(Tank.enemyTank != null)
            {
                EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
                Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank that way we know if we are being chased
                EnemyTankPositionStore.transform.position = -EnemyTankPositionStore.transform.position; //Negate the position of the enemy tank
                Tank.FollowPathToWorldPoint(EnemyTankPositionStore, 1); //We go in the opposite direction of the enemy tank.
                //IF we can make the tank change direction at random intervals to make dodging better.
                return null;
            }

            else
            {
                return typeof(SearchState);
            }
        }
    }

}
