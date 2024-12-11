using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTAttack : MonoBehaviour
{
    private CC_SmartTank Tank;
    //Game object to store enemy position
    GameObject EnemyTankPositionStore = new GameObject();
    int logCounter = 0;
    float fshootTimeLimit = 2f;
    float fkiteTime = 10f;
    float baseDeadTimer = 2.20f;
    float t;
    float fSpeed = 0.8f;
    float fshootT;
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

            if (Tank.priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.HEALTH))
            {
                // Debug.Log("attack switch to retreat low health " + logCounter);
                logCounter++;
                //return typeof(Retreat);
            }

            if (Tank.priorityManager.checkHigh(PRIORITIES.FUEL) && Tank.priorityManager.checkHigh(PRIORITIES.HEALTH))
            {
                Debug.Log("priorities hit to chase ");
                if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > Tank.TankFiringDistance
                   && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
                {
                    Debug.Log("attack switch to chase tank out of firing range " + logCounter);
                    logCounter++;
                    //return typeof(Chase);

                }
            }


            //fire at the stored position
            Tank.TurretFireAtPoint(Tank.LastKnownEPos);
            // return null since the state doesn't change, we will continue attacking
            //return null;


        }


        if (Tank.enemyBase != null && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
        {
            Debug.Log("Firing enemy base");
            t += Time.deltaTime;
            if (baseDeadTimer < t)
            {
                Debug.Log("base dead");
                isFiringAtBase = false;
                t = 0.0f;
            }
            //Potential to do 
            /*   GameObject inverseEnemeyBase = new GameObject();
               inverseEnemeyBase.transform.position = new Vector3(Tank.transform.forward.x , 0, Tank.transform.position.z + -Tank.transform.forward.z*5.0f);
               */
            /*if (Tank.stopAndCheckPos(inverseEnemeyBase, 2.5f,Tank.enemyTank)) {*/

            /* if(Tank.enemyTank != null)
             {
                 Debug.Log("saw enemy tank before attacking base ");
                 return null;
             }*/
            Debug.Log("Attacking enemy base");
            if (isFiringAtBase != true)
            {
                Tank.TurretFireAtPoint(Tank.EnemyBasePos);
                isFiringAtBase = true;
            }



            Debug.Log("go into search after firing at base preventing chase with timer bug");

            //return null;
        }

        Debug.Log("is enemy base null");
        Debug.Log("attack switch to search no condtion was hit " + logCounter);
        //return typeof(SearchState);


        /*  Debug.Log("attack switch to search low on res " + logCounter);
          logCounter++;
          return typeof(SearchState);*/


    }
}
