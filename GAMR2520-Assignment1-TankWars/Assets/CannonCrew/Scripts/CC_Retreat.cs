using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;

public class Retreat : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    GameObject BasePositionStore = new GameObject();    
    float t;
    float fSpeed;
    bool bEnemySeen;

    public Retreat(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Retreat");
        t = 0;
        fSpeed = 1f;
        bEnemySeen = false;
        return null;
    }

    public override Type Exit()
    {
        t = 0;
        fSpeed = 1f;
        bEnemySeen = false;
        return null;
    }

    public override Type Update()
    {
        //Makes sure that none of the tank resources are in the major or critical states
        //If the tank is fine then we go back to the search state to go looking for the enemy tank
        if(Tank.priorityManager.checkHigh(PRIORITIES.HEALTH) && 
           Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            return typeof(SearchState);
        }

        //If we are low on a certain resource
        else
        {
            //We check if we have seen the enemy tank.
             if(Tank.enemyTank != null)
             {
                Debug.Log("Seen Tank");
                 //Set this boolean to true
                 bEnemySeen = true;
                 BasePositionStore.transform.position = Tank.getBasePosition();

                Debug.Log(BasePositionStore.transform.position);
                 EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
                 Tank.FollowPathToWorldPoint(BasePositionStore, fSpeed); //We go in the opposite direction of the enemy tank.
                 Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank that way we know if we are being chased
                 //IF we can make the tank change direction at random intervals to make dodging better.
                 return null;
             }

             else
             {
                 //If we did see the enemy
                 if(bEnemySeen)
                 {
                    Debug.Log("Going back to base");
                    BasePositionStore.transform.position = Tank.getBasePosition();
                    //We generate a new random point in the world
                    //Tank.GenerateNewRandomWorldPoint();
                     
                       
                    //Travel to the that random point but whilst slowing down.
                    fSpeed = fSpeed * 0.25f;
                    //Tank.FollowPathToRandomWorldPoint(fSpeed);

                    Tank.FollowPathToWorldPoint(BasePositionStore, fSpeed);
                    Tank.TurretFaceWorldPoint(EnemyTankPositionStore);


                    //Once we have slowed down to about half speed, we stop the tank. 
                    if(Tank.enemyTank == null)
                    {
                        Tank.TankStop();
                        t += Time.deltaTime;
                    }

                     //We then wait 3 seconds to pass to make sure that the enemy tank isn't anywhere near us.
                     //If 3 seconds pass uninterrupted then we go back to the search state
                     if (t >= 3f)
                     {
                         return typeof(SearchState);
                     }

                     //Other wise we stay in the retreat state
                     else
                     {
                         return null;
                     }
                 }

                 //If we never saw the enemy in the first place then we dont need to stop and wait. We just go back to the search state straight away.
                 else
                 {
                     return typeof (SearchState);
                 }

             }

            return null;
        }
    }

}
