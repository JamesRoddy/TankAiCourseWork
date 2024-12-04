using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;

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
            //We check if we have seen the enemy tank and that we still have bases alive.
             if(Tank.enemyTank != null)
             {
                Debug.Log("Seen Tank");
                 //Set this boolean to true
                 bEnemySeen = true;

                //If bases are alive we go to them.
                if(Tank.getBasePosition() != Vector3.zero)
                {
                    BasePositionStore.transform.position = Tank.getBasePosition();
                    Debug.Log(BasePositionStore.transform.position);
                    EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
                    Tank.FollowPathToWorldPoint(BasePositionStore, fSpeed); //We go in the opposite direction of the enemy tank.
                    Tank.TurretFaceWorldPoint(EnemyTankPositionStore);//Make the turret face the enemy tank that way we know if we are being chased
                                                                      //IF we can make the tank change direction at random intervals to make dodging better.
                    return null;
                }

                //Otherwise we can see the tank but our bases are destroyed so we go into the opposite position of the enemy
                else
                {
                    EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;//Store the position of the enemy tank
                    EnemyTankPositionStore.transform.position = -EnemyTankPositionStore.transform.position;
                    Tank.FollowPathToWorldPoint(EnemyTankPositionStore, fSpeed); //We go in the opposite direction of the enemy tank.
                    return null;
                }
               
             }

             //If we can no longer see the tank
             else
             {
                 //If we did see the enemy
                 if(bEnemySeen)
                 {
                    
                    //And our bases havent been destroyed we go back our base.
                    if (Tank.getBasePosition() != Vector3.zero)
                    {
                        BasePositionStore.transform.position = Tank.getBasePosition();
                        Debug.Log("Going back to base");
                        Tank.FollowPathToWorldPoint(BasePositionStore, fSpeed);

                        //If we cant see the tank and we are close to the base
                        if (Tank.enemyTank == null && Vector3.Distance(Tank.transform.position, BasePositionStore.transform.position) < 100f)
                        {
                            //We stop
                            Debug.Log("Stopping Tank");
                            Tank.TankStop();
                            Tank.FollowPathToWorldPoint(BasePositionStore, 0f);
                            t += Time.deltaTime;

                            //We then wait 3 seconds to pass to make sure that the enemy tank isn't anywhere near us.
                            //If 3 seconds pass uninterrupted then we go back to the search state
                            if (t >= 3f)
                            {
                                return typeof(SearchState);
                            }

                            //Other wise we stay in the retreat state and run to a random point on the map.
                            else
                            {
                                Tank.GenerateNewRandomWorldPoint();
                                Tank.FollowPathToRandomWorldPoint(fSpeed);
                                return null;
                            }
                        }

                        else{ return null; }

                    }

                    //If we never saw the enemy and our bases have been destroyed 
                    else if(Tank.getBasePosition() == Vector3.zero)
                    {
                        //We do see the enemy we run away
                        if(Tank.enemyTank != null)
                        {
                            EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;
                            EnemyTankPositionStore.transform.position = -EnemyTankPositionStore.transform.position;
                            Tank.FollowPathToWorldPoint(EnemyTankPositionStore, fSpeed);
                            Tank.TurretFaceWorldPoint(EnemyTankPositionStore);
                            return null;
                        }

                        //Otherwise we go to the search state
                        else if (Tank.enemyTank == null)
                        {
                            return typeof(SearchState);
                        }

                        else
                        {
                            Tank.GenerateNewRandomWorldPoint();
                            Tank.FollowPathToRandomWorldPoint(fSpeed);
                            return null;
                        }

                    }

                    else { return null; }
                    
                 }

                 //If we never saw the enemy in the first place then we dont need to stop and wait. We just go back to the search state straight away.
                 else
                 {
                     return typeof (SearchState);
                 }

             }

        }
    }

}
