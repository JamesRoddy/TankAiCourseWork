using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
// wrapper for all actions the behaviour tree can perform when in attack state

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
        // taking into account firing delay and a bit extra to stop tank from shooting base twice 
        baseTimerIncrement+= Time.deltaTime;
        if (baseDeadTimer < baseTimerIncrement)
        {
            /*                Debug.Log("base dead");
            */
            isFiringAtBase = false;
            baseTimerIncrement= 0.0f;
            return;
            
        }
        Tank.TurretFaceWorldPoint(Tank.enemyBase);
        if (isFiringAtBase != true)
        {

            Tank.TurretFireAtPoint(Tank.enemyBase);
            isFiringAtBase = true;
            return;
        }
        baseTimerIncrement = 0.0f;


    }

    public void attackEnemy()
    {
        Debug.Log("attacking enemy");



        if (!Tank.TankIsFiring()) // we will ocntsanly try to shoot the enemy after our firing delay 
        {
            Tank.TurretFireAtPoint(Tank.enemyTank);

        }


    }

}
