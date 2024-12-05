using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
using static UnityEditor.ShaderData;
using static UnityEngine.EventSystems.EventTrigger;

public class Retreat : BaseST
{

    private CC_SmartTank Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    GameObject BasePositionStore = new GameObject();
    GameObject safetySpot = new GameObject();
    float runTime = 10.0f;
    float safteySpotDistThresh = 5.0f;
    float safteySpotTimer = 0.0f;
    float retreatCheckDistance = 40.0f;
    float retreatToBaseViableDistance = 30.0f;
    float t;
    float fSpeed;
    bool bEnemySeen = true;
    int logCounter = 0;
    public Retreat(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        safetySpot.transform.position = Vector3.zero;
        Debug.Log("Entered Retreat " + logCounter);
        logCounter++;
        t = 0;
        fSpeed = 1f;
        bEnemySeen = false;
        return null;
    }

    public override Type Exit()
    {
        Debug.Log("Retreat Exit " + logCounter);
        logCounter++;
        safetySpot.transform.position = Vector3.zero;
        t = 0;
        fSpeed = 1f;
        bEnemySeen = false;
        return null;
    }

    

    public override Type Update()
    {
        BasePositionStore.transform.position = Tank.getBasePosition();
        //Makes sure that none of the tank resources are in the major or critical states
        //If the tank is fine then we go back to the search state to go looking for the enemy tank
        if (Tank.priorityManager.checkHigh(PRIORITIES.HEALTH) && 
           Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            Debug.Log("retreat switch to search high on fuel and health"+logCounter);
            logCounter++;
            return typeof(SearchState);
        }

          //If we are low on a certain resource
          //We check if we have seen the enemy tank and that we still have bases alive.
         if(Tank.enemyTank != null)
             {
                Debug.Log("Seen Tank ");
                 //Set this boolean to true
                

                //If bases are alive we go to them.
                if(Tank.getBasePosition() != Vector3.zero && isRetreatToBaseViable())
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
                
               
                findInversionToETank(Tank.LastKnownEPos.transform.position);
                Tank.FollowPathToWorldPoint(safetySpot, fSpeed); //We go in the opposite direction of the enemy tank.
                resetSafteySpot(); // if we get too close to the saftey spot and we are still being chased reset it to be set again(above) 
                return null;
                
               
             }
           
          

            //If we can no longer see the tank
            //And our bases havent been destroyed we go back our base.
            if ( BasePositionStore.transform.position != Vector3.zero )
            {
               
               //If we cant see the tank and we are close to the base
               if (Tank.enemyTank == null  && !bEnemySeen)
               {
                  Debug.Log(Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.transform.position));
                 if (Tank.stopAndCheckPos(Tank.LastKnownEPos, 2.0f, Tank.enemyTank)) {
                    // We then wait 2 seconds to pass to make sure that the enemy tank isn't anywhere near us.
                    //If 2 seconds pass uninterrupted then we go back to the search state
                    
                      if (Tank.enemyTank == null)
                      {
                        Debug.Log("retreat switch to search based on timer " + logCounter);
                        logCounter++;
                        return typeof(SearchState);
                      }
                       bEnemySeen = true;
                       t = 0;

                    /*//Other wise we stay in the retreat state and run to a random point on the map.
                      else
                      {
                        Tank.GenerateNewRandomWorldPoint();
                        Tank.FollowPathToRandomWorldPoint(fSpeed);
                        return null;
                      }*/

                }
                else if ( (Tank.enemyTank != null || bEnemySeen) 
                        && ( Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) <= retreatCheckDistance))
                {

                    t += Time.deltaTime;
                 
                    Debug.Log("Going back to base ");
                    Debug.Log("retreat time " + t);
                    Tank.FollowPathToWorldPoint(BasePositionStore, fSpeed);

                    if(Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > retreatCheckDistance)
                    {
                        bEnemySeen = false;
                       
                    }
                }
                return null;



             }

             return null;
            }
                    //If we never saw the enemy and our bases have been destroyed 
            else if(Tank.getBasePosition() == Vector3.zero)
            {
                        //We do see the enemy we run away
                  if(Tank.enemyTank != null)
                  {

                  /*  EnemyTankPositionStore.transform.position = Tank.enemyTank.transform.position;
                  EnemyTankPositionStore.transform.position = -EnemyTankPositionStore.transform.position;*/
                    findInversionToETank(Tank.LastKnownEPos.transform.position);
                    Tank.FollowPathToWorldPoint(safetySpot, fSpeed);
                    resetSafteySpot();             
                    return null;
                  }

                        //Otherwise we go to the search state
                  else if (Tank.enemyTank == null && Vector3.Distance(Tank.transform.position,Tank.LastKnownEPos.transform.position) > retreatCheckDistance)
                  {
                    if (Tank.stopAndCheckPos(Tank.LastKnownEPos, 3.5f, Tank.enemyTank))
                    {
                       if (Tank.enemyTank == null)
                       {
                        Debug.Log("retreat switch to search based on timer " + logCounter);
                        logCounter++;
                        return typeof(SearchState);

                       }
                      return null;
                    }
                  }

                 /* else
                  { 
                    
                     Tank.GenerateNewRandomWorldPoint();
                     Tank.FollowPathToRandomWorldPoint(fSpeed);
                     return null;
                  }*/
                  return null;
            }
                //If we never saw the enemy in the first place then we dont need to stop and wait. We just go back to the search state straight away.
            else
            {
                // no condtion was hit in retreat moving to search 
                Debug.Log("no condtion was hit in retreat moving to search "+ logCounter);
                logCounter++;
                return typeof (SearchState);
            }

    }




    private void resetSafteySpot()
    {
        if (safetySpot.transform.position != Vector3.zero)
        {
            if (Vector3.Distance(Tank.transform.position, safetySpot.transform.position) < safteySpotDistThresh)
            {
                safetySpot.transform.position = Vector3.zero;
            }

            


        }
    
        


    }
   private void findInversionToETank(Vector3 enemyTankPos)
    {
        
        if(safetySpot.transform.position == Vector3.zero)
        {
            Debug.LogWarning("has invereted enemy position to retreat");
            safetySpot.transform.position = -new Vector3(enemyTankPos.x, 0, enemyTankPos.z);
            
        }


    }

    // is it safe to retreat to base 
    private bool isRetreatToBaseViable()
    {
        if ( !(Vector3.Distance(BasePositionStore.transform.position, Tank.transform.position) > retreatToBaseViableDistance 
            && Vector3.Distance(Tank.transform.position, BasePositionStore.transform.position) < 
            Vector3.Distance(Tank.LastKnownEPos.transform.position, BasePositionStore.transform.position))){

            Debug.Log(" is not safe to retreat to base ");
        }
        else
        {
            Debug.Log("is safe to retreat to base ");

        }

        return Vector3.Distance(BasePositionStore.transform.position, Tank.transform.position) > retreatToBaseViableDistance && Vector3.Distance(Tank.transform.position,BasePositionStore.transform.position)<Vector3.Distance(Tank.LastKnownEPos.transform.position,BasePositionStore.transform.position) ;
    }



}
