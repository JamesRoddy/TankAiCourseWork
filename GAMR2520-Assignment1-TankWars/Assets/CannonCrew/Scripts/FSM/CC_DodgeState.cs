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

    public CC_DodgeState(CC_SmartTank newTank)
    {
        Tank = newTank;
    }

    public override Type Entry()
    {
        if (Tank.enemyTank != null && Vector3.Dot(Tank.enemyTank.transform.forward, Tank.transform.forward) > 0) // if the tank is already not facing us we immidealty go into the chase state
        {
            return typeof(CC_Chase);
        }
        enemyTarget = Tank.LastKnownEPos;
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = enemyTarget.transform.position + (Vector3.Normalize(enemyTarget.transform.position) * orbitRadius);

        orbitPath.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * orbitRadius, 0.0f, Mathf.Cos(Time.realtimeSinceStartup) * orbitRadius); //deine am initial curve around the enemy in attempt to doge a shot
        tankPosition.transform.position = enemyTarget.transform.position + orbitPath.transform.position;

        return null;
    }

    public override Type Update()
    {
        //Check the posititon of the enemy tank
        enemyTarget = Tank.LastKnownEPos;
        Tank.stopAndCheckPos(enemyTarget, 0.5f,Tank.enemyTank,ref waitTime);
        

        if (Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.transform.position) > Tank.TankFiringDistance)
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
