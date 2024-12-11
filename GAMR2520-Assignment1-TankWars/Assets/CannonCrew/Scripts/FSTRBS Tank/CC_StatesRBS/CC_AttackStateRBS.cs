using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class CC_AttackStateRBS : BaseST
{
    private CC_SmartTankRBS Tank;
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
    public CC_AttackStateRBS(CC_SmartTankRBS newTank)
    {
        Tank = newTank;
    }
    public override Type Entry()
    {
        Debug.Log("Attack Enter " + logCounter);
        logCounter++;

        Tank.stats["attackState"] = true;

        return null;
    }

    public override Type Update()
    {
        foreach (var item in Tank.rules.GetRules) // iterates through the rules
        {


            if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
            {
                return item.CheckRule(Tank.stats); // return the state
            }
        }


        if (Tank.stats["enemySeen"] == true && Tank.stats["withinRange"] == true) // if we see the enemy tank
        {

            //fire at the stored position
            Tank.TurretFireAtPoint(Tank.LastKnownEPos);
            // return null since the state doesn't change, we will continue attacking
            return null;


        }


        if (Tank.stats["enemyBaseSeen"] && !Tank.stats["ammoCritical"])
        {

            t += Time.deltaTime;
            if (baseDeadTimer < t)
            {
/*                Debug.Log("base dead");
*/                isFiringAtBase = false;
                t = 0.0f;
            }
            Tank.TurretFaceWorldPoint(Tank.enemyBase);
            if (isFiringAtBase != true)
            {

                Tank.TurretFireAtPoint(Tank.enemyBase);
                isFiringAtBase = true;
            }




            return null; ;
        }

       Debug.Log("is enemy base null");
       Debug.Log("attack switch to search no condtion was hit " + logCounter);
         // reurn default state if none of the conditons are met 
       return typeof(SearchStateRBS);


        


    }

    public override Type Exit()
    {
        /*if(bCalc == false)
        {
            kitePath.transform.position = new Vector3(Mathf.Sin(Time.realtimeSinceStartup) * fkiteRadius, 0.0f, Mathf.Cos(Time.realtimeSinceStartup) * fkiteRadius);
            kiteTankPosition.transform.position = Tank.LastKnownEPos.transform.position + kitePath.transform.position;
            bCalc = true;
        }*/
        Debug.Log("Attack Exit " + logCounter);
        fshootT = 0f;
        isFiringAtBase = false;
        t = 0.0f;
        logCounter++;

        Tank.stats["attackState"] = false;
        return null;
    }
}