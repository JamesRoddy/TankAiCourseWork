using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_BTNewRetreat : MonoBehaviour
{
    private CC_SmartTank Tank;
    GameObject safetySpot = new GameObject();
    bool hasCalculatedEnemyInversion = false;
    float runTime = 10.5f;
    bool changeSafteySpot = false;
    float retreatCheckDistance = 40.0f;
    float retreatToBaseViableDistance = 90.0f;
    float retreatToInvertedEnemtViableDistance = 15.0f;
    bool hasPositionReference = true;
    float t = 0f;
    float waitingTimeForEnemyReference;
    float tankCheckBehindTime = 1.0f;
    float waitTime = 0.0f;
    float fSpeed = 1f;
    bool bEnemySeen = true;
    public bool bReturn = false;

    public CC_BTNewRetreat(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    private bool baseIsViable()
    {
        /*Checks to make sure that returning to our bases is safe.
         * We check if the enemy is looking at us and if we are looking at our bases.
         * We check if both of those things are not behind us.
         * We check if the base isn't too far away and if the enemy isn't too close to it.
         * If all these checks are true then our retreat to the base is viable.
         */
        float dotBetweenUsAndEnemy = Vector3.Dot(Tank.transform.forward, Tank.EtankLastKnownTransformForward);
        float dotBetweenBaseDirAndUs = Vector3.Dot(Tank.transform.forward, Vector3.Normalize(Tank.transform.position - Tank.BasePositionStore));
        bool baseAndTankIsNotBehindANotUs = !(dotBetweenUsAndEnemy >= 0 && dotBetweenBaseDirAndUs >= 0);
        bool baseIsAtHighDistance = Vector3.Distance(Tank.transform.position, Tank.BasePositionStore) > retreatToBaseViableDistance;
        bool enemyTankToCLoseToBase = Tank.compareDistanceBetwenPoints(Tank.BasePositionStore, Tank.LastKnownEPos.transform.position);
        return baseIsAtHighDistance && enemyTankToCLoseToBase && baseAndTankIsNotBehindANotUs;

    }

    public void Update()
    {
        //If we are low on a certain resource
        //We check if we have seen the enemy tank and that we still have bases alive.
        if (bEnemySeen)
        {
            getSafetySpot(); // calculate the safest point using the base position as default if we can(as it it not unknown)

            if (safetySpot.transform.position == Tank.BasePositionStore || hasPositionReference)
            {
                Tank.FollowPathToWorldPoint(safetySpot, fSpeed); //We go in the opposite direction of the enemy tank.
            }

            if (hasPositionReference)
            {
                t -= Time.deltaTime; 
            }

            if (t <= 0.0f)
            {
                bEnemySeen = false;
            }


        }

        //If we can no longer see the tank
        if (!bEnemySeen)
        {
                //We stop the tank and face the turret to be behind us to see if the enemy is still behind us.
                //We do this for 1 second and then go back into search.
                if (Tank.stopAndCheckPos(Tank.LastKnownEPos, tankCheckBehindTime, Tank.enemyTank, ref waitTime))
                {

                    if (Tank.enemyTank == null) // if we didnt see the tank when we retreated 
                    {
                        bReturn = true;
                    }
                    else if (Tank.enemyTank != null)
                    {

                        t = runTime;// set retreat timer  ready for next run 
                        bEnemySeen = true; // assume we saw the enemy

                    }

                }

        }
    }


    private void getSafetySpot()
    {

        if (baseIsViable()) // check if the enemy tank is too close for us to retreat to base 
        {
            safetySpot.transform.position = Tank.BasePositionStore; // if the base last known pos is viable to retreat to we use the base postion as the retareat spot as that is a known position
        }

        else // other wise we invert the enemy tank position and recalaculate it if necessary 
        {
            if (!hasCalculatedEnemyInversion) // check if weve not already inverted the position so our saftey spot isnt ocnstantly changing when it does not need to 
            {
                findInversionToETank(Tank.LastKnownEPos.transform.position); // find inversion
                hasCalculatedEnemyInversion = true; // has inversion
            }

            if (isRetreatToNotSpotViable(Tank.transform.position, safetySpot.transform.position, retreatToInvertedEnemtViableDistance)) // inverted spot no longer safe 
            {
                hasPositionReference = false; 
                bReturn = true;

                Tank.stopAndCheckPos(Tank.LastKnownEPos, 2.0f, Tank.enemyTank, ref waitingTimeForEnemyReference); // checl behind us to see if we can get a reference to the enemy 

                if (Tank.enemyTank != null) // if we could get a refernce meaning they are still close or chasing 
                {
                    findInversionToETank(Tank.LastKnownEPos.transform.position); // calculate new inversio 
                    bEnemySeen = true; // saw enemy
                    hasPositionReference = true; // we have a new refernce to enemy pos
                    hasCalculatedEnemyInversion = true; // have calculated inversion
                    t = runTime; /// get ready to tun 
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
        Vector3 directionToTravel = Vector3.Normalize(Tank.transform.position - pos);
        float directionToTravelInX = directionToTravel.x >= 0 ? 1.5f * retreatCheckDistance : -1.0f * (1.5f * retreatCheckDistance);
        safetySpot.transform.position = new Vector3(directionToTravelInX, 0, (directionToTravel.z * (retreatCheckDistance * 2.0f)));
    }


    // is it safe to retreat to base 
    private bool isRetreatToNotSpotViable(Vector3 position, Vector3 positionOfRetreat, float viableDistance)
    {
        changeSafteySpot = isToCloseToSafetySpotToRetreat(position, positionOfRetreat, viableDistance);
        return changeSafteySpot;
    }

}

