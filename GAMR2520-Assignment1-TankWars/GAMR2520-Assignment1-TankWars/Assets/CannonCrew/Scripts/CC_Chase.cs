using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class Chase : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore;
    float fSpeed = 1f;

    public Chase(CC_SmartTank tank)
    {
        Tank = tank;
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
        if (Tank != null)
        {
            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
            Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank that way we know if we are being chased
            Tank.FollowPathToWorldPoint(EnemyTankPositionStore, fSpeed);
        }

        return null;
    }
}
