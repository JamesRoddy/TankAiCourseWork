using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_AttackState : BaseST
{
    private CC_SmartTank Tank;
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
        EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;

        if (Tank.enemyTanksFound != null)
        {
            Tank.TurretFireAtPoint(EnemyTankPositionStore);
            return null;
        }
        else if (EnemyTankPositionStore )
        {

        }
    }

    public override Type Exit()
    {
        return null;
    }
}
