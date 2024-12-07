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
    float runTime = 6.0f;
    bool changeSafteySpot = false;
    float safteySpotDistThresh = 5.0f;
    float safteySpotTimer = 0.0f;
    float retreatCheckDistance = 40.0f;
    float retreatToBaseViableDistance = 30.0f;
    float t;
    float finalWaitTime = 0.0f;
    float tankCheckBehindTime = 3.0f;
    float waitTime = 0.0f;
    float fSpeed;
    bool bEnemySeen = false;
    int logCounter = 0;
    public Retreat(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        safetySpot.transform.position = Tank.BasePositionStore;
        Debug.Log("Entered Retreat " + logCounter);
        logCounter++;
        t = 0.0f;
        waitTime = 0.0f;
        fSpeed = 1f;
        changeSafteySpot = false;
        bEnemySeen = false;
        return null;
    }

    public override Type Exit()
    {
        Debug.Log("Retreat Exit " + logCounter);
        logCounter++;
        safetySpot.transform.position = Tank.BasePositionStore;
        changeSafteySpot = false;
        t = 0.0f;
        waitTime = 0.0f;
        fSpeed = 1.0f;
        bEnemySeen = false;
        return null;
    }



    public override Type Update()
    {
        


        Debug.Log("saftey spot current pos "+safetySpot.transform.position);

        //If we are low on a certain resource
        //We check if we have seen the enemy tank and that we still have bases alive.
        if (bEnemySeen)
        {
           getSafetySpot(); // calculate the safest point using the base position as default if we can(as it it not unknown)
           Debug.Log(safetySpot.transform.position);
           Tank.FollowPathToWorldPoint(safetySpot, fSpeed); //We go in the opposite direction of the enemy tank.


            t -= Time.deltaTime; // decrement timer to look back againn
            Debug.Log(" enemy seen decrment retreat time " + t );

            if (t <= 0.0f)
            {
                bEnemySeen = false;
            }
        


        }



        //If we can no longer see the tank
       
        if (!bEnemySeen  )
        {
            Debug.Log("stopping distance "+Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.transform.position));
            //  wait 2 seconds to pass to make sure that the enemy tank isn't anywhere near us.
            //If 2 seconds pass uninterrupted then we go back to the search state
            Debug.Log("t before entering retreat swivile " + t);
           
            Debug.Log("tank runtime met  t was "+t);
            if (Tank.stopAndCheckPos(Tank.LastKnownEPos, tankCheckBehindTime, Tank.enemyTank,ref waitTime))
            {

                t = runTime;// set retreat timer  ready for next run 
            

                if (Tank.enemyTank == null) // if we didnt see the tank when we retreated 
               {
                        Debug.Log("retreat switch to search on timer enemy not seen" + logCounter);
                        logCounter++;
                        bEnemySeen = false;

                        Debug.Log("executing final retreat check for " + tankCheckBehindTime + "seconds");
                        if(Tank.enemyTank == null)
                        {
                            return typeof(SearchState); // go into search
                        }

                    return null;
                    
                    
               }
               else if(Tank.enemyTank != null )
                {

                   Debug.Log("enemy tank was not null when checking retreat");
                 
                   getSafetySpot(); // recalculate saftey spot when seen enemy 
                   Debug.Log("retreat timer  " + t);
                    Debug.Log(" retreat timer set equal to runtime t was : " + t);
                    bEnemySeen = true; // assume we saw the enemy
                
               }
               return null;

                
            }
            return null;


        }


        /*if ((Tank.enemyTank != null || bEnemySeen)) // we check again after we look behind us to prevent us from just stopping 
        {

            t += Time.deltaTime;
            getSafetySpot();
            Debug.Log("Going back to base ");
            Tank.FollowPathToWorldPoint(safetySpot, fSpeed);

            if (Vector3.Distance(Tank.transform.position, Tank.LastKnownEPos.transform.position) > retreatCheckDistance)
            {
                bEnemySeen = false;

            }
        }*/

        return null;
    }

   
    private void getSafetySpot()
    {

        if (isRetreatToSpotViable(Tank.transform.position, Tank.BasePositionStore)) // check if the enemy tank is too close for us to retreat to base 
        {

            safetySpot.transform.position = Tank.BasePositionStore; // if the base last known pos is viable to retreat to we use the base postion as the retareat spot as that is a known position
            Debug.Log("saftey spot set to base " + safetySpot.transform.position);


        }
        else //  we find the correct inversion to the enemy tanks position and move to that instead
        {
            Debug.Log("not safe to retreat to base ");
            findInversionToETank(Tank.LastKnownEPos.transform.position);


        }
/*        safetySpot.transform.position = safetySpot.transform.position + new Vector3(MathF.Sin(Time.realtimeSinceStartup) * 10.0f, 0.0f, 0.0f);
*/        //occsilate position to dodge;

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



    private bool isToCloseToSafetySpotToRetreat(Vector3 positionOfTank, Vector3 positionOfRetreat)
    {


        return Vector3.Distance(positionOfTank, positionOfRetreat) < retreatToBaseViableDistance;
    }

    private void findInversionToETank(Vector3 pos)
    {
        
       
            Debug.Log("has invereted saftey position to retreat");
            Vector3 directionToTravel = Vector3.Normalize
                (  Tank.transform.position - pos );
            float directionToTravelInX = directionToTravel.x >= 0 ? 1.5f * retreatCheckDistance : -1.0f * (1.5f * retreatCheckDistance); 
            
            
             
            Debug.Log("normalzied direction vector from us to enemy z negated " + new Vector3(1-directionToTravel.x,0,-directionToTravel.z ));
            safetySpot.transform.position = new Vector3(directionToTravelInX , 0, -(directionToTravel.z * (retreatCheckDistance*2.0f)));
            Debug.Log("inverted spot " + safetySpot.transform.position);
           



        


    }


    // is it safe to retreat to base 
    private bool isRetreatToSpotViable(Vector3 position, Vector3 positionOfRetreat)
    {
        changeSafteySpot = (!(Tank.compareDistanceBetwenPoints(positionOfRetreat, Tank.LastKnownEPos.transform.position) || (isToCloseToSafetySpotToRetreat(position, positionOfRetreat) && ( Tank.enemyTank != null 
            || bEnemySeen ))));
        if (!changeSafteySpot)
        {

            Debug.Log(" is  safe to retreat to base ");
        }
        else
        {
            Debug.Log("is not safe to retreat to base use inversion of e tank");

        }

        return !changeSafteySpot;
    }

}
