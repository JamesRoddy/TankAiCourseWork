using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
// wrapper for all actions the behaviour tree can perform when in attack state

public class BTAttackActions : MonoBehaviour
{

    CC_smartTankFSMRBSBT Tank;

    public float baseDeadTimer = 2.10f;
    public float baseTimerIncrement = 0.0f;
    bool isFiringAtBase = false;
    public BTAttackActions(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
    }


    public void AttackBase()
    {
        // taking into account firing delay and a bit extra to stop tank from shooting base twice 
        baseTimerIncrement+= Time.deltaTime;
        Debug.Log("base timer increment" + baseTimerIncrement);
        if ( baseTimerIncrement >= baseDeadTimer)
        {
             Debug.Log("base dead");
            
            isFiringAtBase = false;
            baseTimerIncrement= 0.0f; 

            return;
            
        }
        
        if (isFiringAtBase != true)
        {
            Debug.Log("queue up attack");
            Tank.TurretFireAtPoint(Tank.enemyBase);
            isFiringAtBase = true;
            return;
        }
  

    }

    public void attackEnemy()
    {
        
        if (!Tank.TankIsFiring()) // we will ocntsanly try to shoot the enemy after our firing delay 
        {
            Debug.Log("attack enemy");
            Tank.TurretFireAtPoint(Tank.enemyTank);

        }


    }



    public void ResetTimer()
    {
        baseTimerIncrement = 0.0f;
    }

}
