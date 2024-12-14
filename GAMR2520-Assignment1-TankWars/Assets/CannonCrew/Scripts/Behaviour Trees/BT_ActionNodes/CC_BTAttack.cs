using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTAttack : MonoBehaviour
{
    private CC_SmartTank Tank;
    float baseDeadTimer = 2.20f;
    float t;
    bool isFiringAtBase = false;
    public CC_BTAttack(CC_SmartTank newTank)
    {
        Tank = newTank;
    }

    // Update is called once per frame
    public void Update()
    {
        if (Tank.enemyTank != null) // if we see the enemy tank
        {
            //fire at the stored position
            Tank.TurretFireAtPoint(Tank.LastKnownEPos);
        }

        //If we see the enemy base we fire at that aswell.
        if (Tank.enemyBase != null && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
        {
            //We only fire on it for a certain amount of seconds because otherwise we attack for too long.
            //Wasting ammo and time which lets us get hit in the process.
            t += Time.deltaTime;
            if (baseDeadTimer < t)
            {
                isFiringAtBase = false;
                t = 0.0f;
            }
            
            if (isFiringAtBase != true)
            {
                Tank.TurretFireAtPoint(Tank.EnemyBasePos);
                isFiringAtBase = true;
            }

        }

    }
}
