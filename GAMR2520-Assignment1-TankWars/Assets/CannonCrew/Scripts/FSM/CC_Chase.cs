using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class CC_Chase : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;
    float tankAttackMinThresh = 20.0f;
    float chaseTime = 2.0f;
    float t = 0.0f;
    bool hasSeenBase = false;
    int logCounter = 0;
    public CC_Chase(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        logCounter++;
        fSpeed = 1f;
        t = 0.0f;
        return null;
    }

    public override Type Exit()
    {
        logCounter++;
        t = 0.0f;
        fSpeed = 1f;
        return null;
    }

    public override Type Update()
    {



        //First we check for any enemy tanks in our vision
        if (Tank.enemyTank != null && Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            if(Tank.priorityManager.checkQueue(queuePriority.CRITICAL,PRIORITIES.AMMO))
            {
                return typeof(CC_SearchState);
            }


            if (Tank.priorityManager.checkLow(PRIORITIES.FUEL) || Tank.priorityManager.checkLow(PRIORITIES.HEALTH))
            {
                logCounter++;
                return typeof(CC_Retreat);
            }
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot

          

            //if our tank is between max and min units away from the enemy and we are good on fuel, we go into the kite state
             if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < 60f
                && Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > tankAttackMinThresh)
             {
                logCounter++;
                return typeof(CC_DodgeState);
            }

          
        }

  

        //If there are no enemy tnaks in our vision we check for enemy bases
        if (Tank.enemyBase != null || hasSeenBase   )
        {
            hasSeenBase = true;

            //Once we have seen the enemy the base we travel towards it.
            if(Tank.enemyBase != null)// ensure base doesnt slip out of vision
            {
                Tank.FollowPathToWorldPoint(Tank.enemyBase, fSpeed);
            }
            else
            {
                Tank.FollowPathToWorldPoint(Tank.EnemyBasePos, fSpeed);
            }

            //if our tank is between  min units away from the enemy base and we are good on ammo, we go into the attack state
            if (!Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
                if (Vector3.Distance(Tank.transform.position, Tank.EnemyBasePos.transform.position) < Tank.BaseFiringDistance
               && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
                {
                    logCounter++;
                    hasSeenBase = false;
                    return typeof(CC_AttackState);
                }
            }
            else if(Tank.priorityManager.checkQueue(queuePriority.CRITICAL,PRIORITIES.AMMO))
            {
                return typeof(CC_SearchState);
            }
           

            return null;

            

               
        }
        else if (t < chaseTime && Tank.enemyTank == null && Tank.enemyBase == null )
        {
            t += Time.deltaTime;
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot
            return null;
        }

        //Chase the enemy tank once it gets outside of our range
        if (Tank.enemyTank == null)
        {
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot

            if(Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < 5f)
            {
                //Questionable change
                if(Tank.enemyTank != null)
                {
                    Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);
                    Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);

                    if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < Tank.TankFiringDistance
                        && Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > tankAttackMinThresh)
                    {
                        return typeof(CC_AttackState);
                    }

                    else
                    {
                        return null;
                    }
    
                }

                logCounter++;
                return typeof(CC_SearchState);
            }
            return null;
        }

      return typeof(CC_SearchState);

    }
}
