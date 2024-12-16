using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
using static UnityEditor.ShaderData;
using static UnityEngine.EventSystems.EventTrigger;

public class CC_RetreatStateRBS : BaseST
{

    private CC_SmartTankRBS Tank;
    GameObject EnemyTankPositionStore = new GameObject();
    GameObject BasePositionStore = new GameObject();
    GameObject safetySpot = new GameObject();
    bool hasCalculatedEnemyInversion = false;
    float runTime = 10.5f;
    bool changeSafteySpot = false;
    float safteySpotDistThresh = 4.0f;
    float safteySpotTimer = 0.0f;
    float retreatCheckDistance = 40.0f;
    float retreatToBaseViableDistance = 90.0f;
    float retreatToInvertedEnemtViableDistance = 15.0f;
    bool hasPositionReference = true;
    float t;
    float waitingTimeForEnemyReference;
    float tankCheckBehindTime = 1.0f;
    float waitTime = 0.0f;
    float fSpeed;
    bool bEnemySeen = true;
    int logCounter = 0;
    public CC_RetreatStateRBS(CC_SmartTankRBS newtank)
    {
        Tank = newtank;
    }

    public override Type Entry()
    {
        safetySpot.transform.position = Tank.BasePositionStore;
        Debug.Log("Entered Retreat " + logCounter);

        Tank.stats["retreatState"] = true;

        logCounter++;
        t = runTime;
        waitTime = 0.0f;
        fSpeed = 1f;
        changeSafteySpot = false;
        hasPositionReference = true;
        bEnemySeen = true;
        return null;
    }

    public override Type Exit()
    {
        Debug.Log("Retreat Exit " + logCounter);

        Tank.stats["retreatState"] = false;

        logCounter++;
        safetySpot.transform.position = Tank.BasePositionStore;
        changeSafteySpot = false;
        hasPositionReference = true;
        t = runTime;
        waitTime = 0.0f;
        fSpeed = 1.0f;
        bEnemySeen = true;
        hasCalculatedEnemyInversion = false;
        return null;
    }


    private bool baseIsViable()
    {

        float dotBetweenUsAndEnemy = Vector3.Dot(Tank.transform.forward, Tank.EtankLastKnownTransformForward);
        float dotBetweenBaseDirAndUs = Vector3.Dot(Tank.transform.forward, Vector3.Normalize(Tank.transform.position - Tank.BasePositionStore));
        bool baseAndTankIsNotBehindANotUs = !(dotBetweenUsAndEnemy >= 0 && dotBetweenBaseDirAndUs >= 0);
        bool baseIsAtHighDistance = Vector3.Distance(Tank.transform.position, Tank.BasePositionStore) > retreatToBaseViableDistance;
        bool enemyTankToCLoseToBase = Tank.compareDistanceBetwenPoints(Tank.BasePositionStore, Tank.LastKnownEPos.transform.position);
        Debug.Log("base and tank was not behind us " + baseAndTankIsNotBehindANotUs);
        return baseIsAtHighDistance && enemyTankToCLoseToBase && baseAndTankIsNotBehindANotUs;

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
        Debug.Log("saftey spot current pos " + safetySpot.transform.position);
        //If we are low on a certain resource
        //We check if we have seen the enemy tank and that we still have bases alive.
        if (bEnemySeen)
        {

            getSafetySpot(); // calculate the safest point using the base position as default if we can(as it it not unknown)

            if(Tank.enemyTank == null && !hasPositionReference)
            {

                Debug.Log("enemy not seen when trying to get pos reference going to search");
                return typeof(CC_SearchStateRBS);
            }
            Debug.Log(safetySpot.transform.position);
            Debug.Log("does safety spot need to change " + changeSafteySpot);


            if (safetySpot.transform.position == Tank.BasePositionStore || hasPositionReference)
            {
                Tank.FollowPathToWorldPoint(safetySpot, fSpeed); //We go in the opposite direction of the enemy tank.
            }
            



            if (hasPositionReference)
            {

                t -= Time.deltaTime; // decrement timer to look back againn
/*                Debug.Log(" enemy seen decrment retreat time " + t);
*/            }
            else
            {
/*                Debug.Log("has no position reference");
*/
            }

            if (t <= 0.0f)
            {

                bEnemySeen = false;
            }






        }



        //If we can no longer see the tank

        if (!bEnemySeen)
        {
            
                if (Tank.stopAndCheckPos(Tank.LastKnownEPos, tankCheckBehindTime, Tank.enemyTank, ref waitTime))
                {

                    if (Tank.enemyTank == null) // if we didnt see the tank when we retreated 
                    {
                        return typeof(CC_SearchStateRBS); // go into search


                    }
                    else if (Tank.enemyTank != null)
                    {
                        t = runTime;// set retreat timer  ready for next run 
                        bEnemySeen = true; // assume we saw the enemy

                    }
                    return null;

                }


            
            return null;


        }

        foreach (var item in Tank.rules.GetRules) // iterates through the rules
        {
            if (item.CheckRule(Tank.stats) != null) // if a rule doesn't return null
            {
                return item.CheckRule(Tank.stats); // return the state
            }
        }

        return null;
    }


    private void getSafetySpot()
    {

        if (baseIsViable()) // check if the enemy tank is too close for us to retreat to base 
        {
            Debug.Log(Vector3.Distance(Tank.BasePositionStore, Tank.transform.position));

            safetySpot.transform.position = Tank.BasePositionStore; // if the base last known pos is viable to retreat to we use the base postion as the retareat spot as that is a known position
            Debug.Log("saftey spot set to base " + safetySpot.transform.position);


        }
        else // other wise we inverte the enemy tank position and recalaculate it if necessary 
        {
            Debug.Log("not safe to retreat to base ");
            if (!hasCalculatedEnemyInversion) // check if weve not already inverted the position so our saftey spot isnt ocnstantly changing when it does not need to 
            {
                findInversionToETank(Tank.LastKnownEPos.transform.position); // find inversion
                hasCalculatedEnemyInversion = true; // has inversion
            }

            if (   isRetreatToNotSpotViable(Tank.transform.position, safetySpot.transform.position, retreatToInvertedEnemtViableDistance)) // inverted spot no longer safe 
            {
                hasPositionReference = false;
                Debug.Log(hasPositionReference);
                Debug.Log("was not  viable to retreat to inverted enemy spot looking behind for new refernce to enemy");


                Tank.stopAndCheckPos(Tank.LastKnownEPos, 2.0f, Tank.enemyTank, ref waitingTimeForEnemyReference); // checl behind us to see if we can get a reference to the enemy 

                if (Tank.enemyTank != null) // if we could get a refernce meaning they are still close or chasing 
                {
                    findInversionToETank(Tank.LastKnownEPos.transform.position); // calculate new inversio 
                    bEnemySeen = true; // saw enemy
                    hasPositionReference = true; // we have a new refernce to enemy pos
                    hasCalculatedEnemyInversion = true; // have calculated inversion
                    t = runTime; /// get ready to tun 
                    Debug.Log("does have reference " + hasPositionReference + " " + safetySpot.transform.position);
                    Debug.Log("reseting values retreat timer reset to: " + runTime + " enemy seen set to true: " + bEnemySeen);
                    return;
                }



            }
        }

    }





    // is it safe to retreat to base 




    private bool isToCloseToSafetySpotToRetreat(Vector3 positionOfTank, Vector3 positionOfRetreat, float viableDistance)
    {


        return Vector3.Distance(positionOfTank, positionOfRetreat) <= viableDistance;
    }

    private void findInversionToETank(Vector3 pos)
    {

        Debug.Log("has invereted saftey position to retreat");
        Vector3 directionToTravel = Vector3.Normalize(Tank.transform.position - pos);
        float directionToTravelInX = directionToTravel.x >= 0 ? 1.5f * retreatCheckDistance : -1.0f * (1.5f * retreatCheckDistance);
        Debug.Log("normalzied direction vector from us to enemy z negated " + new Vector3(1 - directionToTravel.x, 0, -directionToTravel.z));
        safetySpot.transform.position = new Vector3(directionToTravelInX, 0, (directionToTravel.z * (retreatCheckDistance * 2.0f)));
        Debug.Log("inverted spot " + safetySpot.transform.position);




    }


    // is it safe to retreat to base 
    private bool isRetreatToNotSpotViable(Vector3 position, Vector3 positionOfRetreat, float viableDistance)
    {
        changeSafteySpot = isToCloseToSafetySpotToRetreat(position, positionOfRetreat, viableDistance);
        Debug.Log("  change saftey spot " + Vector3.Distance(position, positionOfRetreat));
        if (!changeSafteySpot)
        {

            Debug.Log(" is  safe to retreat to base ");
        }
        else
        {
            Debug.Log("is not safe to retreat to base use inversion of e tank");

        }

        return changeSafteySpot;
    }

}
