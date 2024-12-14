using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTChase : MonoBehaviour
{
    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    float fSpeed = 1f;
    float tankAttackMinThresh = 20.0f;
    float chaseTime = 2.0f;
    float t = 0.0f;
    bool hasSeenBase = false;
    int logCounter = 0;

    public CC_BTChase(CC_SmartTank newtank)
    {
        Tank = newtank;
    }


    public void Update()
    {

        //If we see the enemy we go cahse them
        if(Tank.enemyTank != null)
        {
            Tank.TurretFaceWorldPoint(Tank.LastKnownEPos);//Make the turret face the enemy tank so that we keep it in our vision
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);//Follow the tank so that we have a more accurate shot
        }
       

        //If there are no enemy tanks in our vision we check for enemy bases
       if (Tank.enemyBase != null || hasSeenBase)
        {
            hasSeenBase = true;
            //Once we have seen the enemy the base we travel towards it.

            if (Tank.enemyBase != null)// ensure base doesnt slip out of vision
            {
                Tank.FollowPathToWorldPoint(Tank.enemyBase, fSpeed);
            }
            else
            {
                Tank.FollowPathToWorldPoint(Tank.EnemyBasePos, fSpeed);
            }

        }
    }

    //If we lose vision of the enemy tank we go to their last sited location to try and find them again.
    public void LostVisionChase()
    {
        Debug.Log("t: " + t);
        if (t < chaseTime && Tank.enemyTank == null && Tank.enemyBase == null)
        {
            t += Time.deltaTime;
            Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, fSpeed);  //Follow the tank so that we have a more accurate shot
        }
    }

}
