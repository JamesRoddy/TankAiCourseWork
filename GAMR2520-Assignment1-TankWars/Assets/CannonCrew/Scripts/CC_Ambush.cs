using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class Ambush : BaseST
{
    CC_SmartTank Tank;
    float fTimeLimit = 15.0f;
    float t;
    float fRotate;
    GameObject tankPosition = new GameObject();
    GameObject orbitPath = new GameObject();
    Vector3 offset = new Vector3();

    //This state is for when we havent seen anything important for 15 seconds.
    //So what it does is stop the tank, saving our precious fuel, and constantly rotate the turret in circles for 15 seconds lying in wait to ambush.
    //Once the enemy tank comes into proximity we ATTACK unless on of our resources is far too low.
    public Ambush(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Update()
    {

        Tank.TankStop();

        t += Time.deltaTime;

        if (t <= fTimeLimit)
        {
            
            fRotate += Time.deltaTime;
            //Rotates the turret over a period of time
            //Need to find a way to adjust the speed.
            orbitPath.transform.position = new Vector3(Mathf.Sin(fRotate), 0.0f, Mathf.Cos(fRotate) * 10f);
            tankPosition.transform.position = tankPosition.transform.position + orbitPath.transform.position;

            Tank.TurretFaceWorldPoint(tankPosition);

            if(Tank.enemyTank != null)
            {
                return typeof(CC_AttackState);
            }
            return null;
           
        }

        return typeof(SearchState);
    }


    public override Type Entry()
    {
        Debug.Log("Entered Camping");
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = Vector3.zero;
        fRotate = 0.0f;
        t = 0.0f;
        return null;
    }

    public override Type Exit()
    {
        Debug.Log("Exited Camping");
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = Vector3.zero;
        fRotate = 0.0f;
        t = 0.0f;
        return null ;
    }

}
