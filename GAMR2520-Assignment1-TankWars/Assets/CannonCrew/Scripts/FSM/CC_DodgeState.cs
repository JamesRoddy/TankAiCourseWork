using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static PriorityManager;

public class CC_DodgeState : BaseST
{
    public CC_SmartTank Tank;
    float tankAttackMinThresh = 10.0f;
    float orbitRadius = 25f;
    float fSpeed = 0.6f;
    float t;
    float cantSeeTankTimer;
    float cantSeeTankTimerThresh = 2.0f;
    float fTimeLimit = 6.25f;
    float waitTime = 0.0f;
    GameObject enemyTarget = new GameObject();
    GameObject tankPosition = new GameObject();
    GameObject orbitPath = new GameObject();
    int engamentCounter = 0;

    public CC_DodgeState(CC_SmartTank newTank)
    {
        Tank = newTank;
    }

    public override Type Entry()
    {
        if (Tank.enemyTank != null && Vector3.Dot(Tank.enemyTank.transform.forward, Tank.transform.forward) >= 0)
        {
            return typeof(CC_AttackState);
        }
        engamentCounter++;
        enemyTarget = Tank.LastKnownEPos;
        if (Tank.hasCollidedWithEnemy) // if we collided with the enmy we will try to snake around them at a wider angle 
        {
            orbitRadius = 60.0f;
        }
        else
        {
            orbitRadius = 25.0f;
        }
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = enemyTarget.transform.position + (Vector3.Normalize(enemyTarget.transform.position) * orbitRadius);

        orbitPath.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * orbitRadius, 0.0f, Mathf.Cos(Time.realtimeSinceStartup) * orbitRadius);
        tankPosition.transform.position = enemyTarget.transform.position + orbitPath.transform.position;

        return null;
    }

    public override Type Update()
    {
        //Check the posititon of the enemy tank
        enemyTarget = Tank.LastKnownEPos;
        Tank.stopAndCheckPos(enemyTarget, 0.5f, Tank.enemyTank, ref waitTime);
        if (Tank.enemyTank == null)
        {

        }

        //TO DO FIX BROKEN TRANSITION BETWEEN DODGE AND CHASE WE CAN END UP REPEATELDY SWITCHING BETWEEN THE TWO 


        if (Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.FUEL) || Tank.priorityManager.checkLow(PRIORITIES.HEALTH)) // if fuel becomes a critcia prioity while we are snaking around the enemy then we retreat 
        {
            return typeof(CC_Retreat);
        }

        if (Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.transform.position) > Tank.TankFiringDistance)// if the tank gets out of our firing range while were didging that means they have made the decision to retreat rather than chase or attack so we will
                                                                                                                       // chase them assusming we either wasted their shots enough to where they 
                                                                                                                       // validated the enagement as not worth it or they lost sight of us while we were moving in and out of their vision
        {
            return typeof(CC_Chase);
        }

        //Start a timer
        t += Time.deltaTime;

        //For 6.25 seconds 
        if (t <= fTimeLimit)
        {
            //We create path of the orbit and then we apply an offset to the enemy position 
            orbitPath.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * orbitRadius, 0.0f, Mathf.Cos(Time.realtimeSinceStartup) * orbitRadius);
            tankPosition.transform.position = enemyTarget.transform.position + orbitPath.transform.position;

            //We then move to that path and as this state is called from chase we will be dodging enemy bullets and always getting the first shot off.
            Tank.TurretFaceWorldPoint(enemyTarget);
            Tank.GeneratePathToWorldPoint(tankPosition);
            Tank.FollowPathToWorldPoint(tankPosition, fSpeed, AStar.HeuristicMode.EuclideanNoSqrt);
            return null;
        }
        Tank.hasCollidedWithEnemy = false;

        return typeof(CC_AttackState);

    }


    public override Type Exit()
    {
        t = 0f;
        enemyTarget = Tank.LastKnownEPos;
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = Vector3.zero;
        return null;
    }

}
