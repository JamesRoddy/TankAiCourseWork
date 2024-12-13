using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class BTAttackActions : MonoBehaviour
{

    CC_smartTankFSMRBSBT Tank;

    float baseDeadTimer = 2.20f;
    float baseTimerIncrement = 0.0f;
    bool isFiringAtBase = false;
    public BTAttackActions(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
    }


    public void AttackBase()
    {
        Debug.Log("attack base");
        baseTimerIncrement += Time.deltaTime;
        if (baseDeadTimer < baseTimerIncrement)
        {
            /*                Debug.Log("base dead");
            */
            isFiringAtBase = false;
            baseTimerIncrement = 0.0f;
        }
        Tank.TurretFaceWorldPoint(Tank.enemyBase);
        if (isFiringAtBase != true)
        {

            Tank.TurretFireAtPoint(Tank.enemyBase);
            isFiringAtBase = true;
        }



    }

    public void attackEnemy()
    {
        Debug.Log("attacking enemy");

        Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);

        if (!Tank.TankIsFiring())
        {
            Tank.TurretFireAtPoint(Tank.enemyTank);

        }


    }

}
