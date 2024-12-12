using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class BTAttackFunctions : MonoBehaviour
{

    CC_SmartTank Tank;

    float baseDeadTimer = 2.20f;
    float baseTimerIncrement = 0.0f;
    bool isFiringAtBase = false;
    public BTAttackFunctions(CC_SmartTank tank)
    {
        this.Tank = tank;
    }


     public void AttackBase()
    {

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
        Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);

        if (!Tank.TankIsFiring())
        {
            Tank.TurretFireAtPoint(Tank.enemyTank);
        
        }


    }

}
