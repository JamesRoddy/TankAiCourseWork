using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static PriorityManager;

public class DodgeState : BaseST
{
    public CC_SmartTank Tank;
    float tankAttackMinThresh = 10.0f;
    float orbitRadius = 25f;
    float fSpeed = 0.6f;
    float t;
    float fTimeLimit = 6.25f;
    GameObject enemyTarget = new GameObject();
    GameObject tankPosition = new GameObject();
    GameObject orbitPath = new GameObject();

    public DodgeState(CC_SmartTank newTank)
    {
        Tank = newTank;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Kite");
        if (Tank.enemyTank != null && Vector3.Dot(Tank.enemyTank.transform.forward, Tank.transform.forward) >= 0)
        {
            Debug.Log("Behinde the tank");
            return typeof(CC_AttackState);
        }
        enemyTarget = Tank.LastKnownEPos;
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
        Tank.stopAndCheckPos(enemyTarget, 0.3f,Tank.enemyTank);
        if (Tank.enemyTank == null)
        {
            Debug.Log("Cant see tank");
        }

        if (Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.transform.position) > Tank.TankFiringDistance)
        {
            return typeof(Chase);
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
        Debug.Log("Exited Kite");
        t = 0f;
        enemyTarget = Tank.LastKnownEPos;
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = Vector3.zero;
        return null;
    }
    
}
