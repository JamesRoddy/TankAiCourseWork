using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class Chase : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed;
    float tankAttackMinThresh = 20.0f;
    float chaseTime = 2.0f;
    float t = 0.0f;
    int logCounter = 0;
    public Chase(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Chase " + logCounter);
        logCounter++;
        fSpeed = 1f;
        t = 0.0f;
        return null;
    }

    public override Type Exit()
    {
        Debug.Log("Chase Exit " + logCounter);
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
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot



            //if our tank is between max and min units away from the enemy and we are good on fuel, we go into the kite state
             if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) < 60f
                && Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > tankAttackMinThresh)
             {
                Debug.Log("switch attack: greater than min attack dist and smaller than max attack dist and not low fuel or ammo " + logCounter);
                logCounter++;
                return typeof(DodgeState);
            }

            if (Tank.priorityManager.checkLow(PRIORITIES.FUEL) || Tank.priorityManager.checkLow(PRIORITIES.HEALTH))
            {
                Debug.Log(" switch retreat due to fuel priority " + logCounter);
                logCounter++;
                return typeof(Retreat);
            }
        }

        if (t < chaseTime && Tank.enemyTank == null && Tank.enemyBase == null)
        {
            Debug.Log("Chasing with timer ");
            t += Time.deltaTime;
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot




            return null;
        }

        //If there are no enemy tnaks in our vision we check for enemy bases
        if (Tank.enemyBase != null )
        {
            //Once we have seen the enemy tbase we travel towards it.
            Debug.Log("Chasing Enemy Bases");
            Tank.TurretFaceWorldPoint(Tank.EnemyBasePos);
            Tank.FollowPathToWorldPoint(Tank.EnemyBasePos, fSpeed);

            //if our tank is between max and min units away from the enemy base and we are good on fuel, we go into the attack state
            if (Vector3.Distance(Tank.transform.position, Tank.EnemyBasePos.transform.position) < Tank.BaseFiringDistance
                && Vector3.Distance(Tank.transform.position, Tank.EnemyBasePos.transform.position) > tankAttackMinThresh
                && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
                Debug.Log("Base switch attack: greater than min attack dist and smaller than max attack dist and not low fuel or ammo " + logCounter);
                logCounter++;
                return typeof(CC_AttackState);
            }

             return null;  
        }


        //Chase the enemy tank once it gets outside of our range
        if(Tank.enemyTank == null)
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
                        Debug.Log("Switches to attack after it tries to chase a retreating tank");
                        return typeof(CC_AttackState);
                    }

                    else
                    {
                        return null;
                    }
    
                }

                Debug.Log("switch search tank no longer visible after moving to last known pos " + logCounter);
                logCounter++;
                return typeof(SearchState);
            }
            return null;
        }

      return typeof(SearchState);

    }
}
