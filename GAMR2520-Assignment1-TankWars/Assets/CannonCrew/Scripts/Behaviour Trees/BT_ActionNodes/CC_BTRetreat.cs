using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTRetreat : MonoBehaviour
{

    private CC_SmartTank Tank;
    private CC_BTWait wait;

    //private BaseAIBehaviourModel transitionContext;
    //private Type stateToReturn = null;
    private GameObject safetySpot = new GameObject();
    private bool hasCalculatedEnemyInversion = false;
    private bool retreatCheckComplete = false;
    private float runTime = 10.5f;
    private bool changeSafteySpot = false;

    private float retreatCheckDistance = 35.0f;
    private float retreatToBaseViableDistance = 90.0f;
    private float retreatToInvertedBaseViableDistance = 15.0f;

    private bool hasPositionReference = true;
    private bool bEnemySeen = true;
    private float retreatCheckTime = 4.0f;
    private float waitingTimeForEnemyReference;
    private float tankCheckBehindTime = 0.5f;
    private float t;
    private float fSpeed = 1f;

    public CC_BTRetreat(CC_SmartTank newTank, CC_BTWait newWait)
    {
        Tank = newTank;
        wait = newWait;
    }


    private bool baseIsViable()
    {



        float dotBetweenUsAndEnemy = Vector3.Dot(Tank.transform.forward, Tank.EtankLastKnownTransformForward);
        float dotBetweenBaseDirAndUs = Vector3.Dot(Tank.transform.forward, Vector3.Normalize(Tank.transform.position - Tank.BasePositionStore));
        bool baseAndTankIsNotBehindANotUs = !(dotBetweenUsAndEnemy >= 0 && dotBetweenBaseDirAndUs >= 0);

        float distanceToBaseOfEnemyTank = Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.BasePositionStore);

        bool baseIsAtHighDistance = Vector3.Distance(Tank.transform.position, Tank.BasePositionStore) > retreatToBaseViableDistance;
        bool enemyTankNotToCLoseToBase = Tank.compareDistanceBetwenPoints(Tank.BasePositionStore, Tank.LastKnownEPos.transform.position);
        float distanceToBaseAndEnemy = Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.BasePositionStore);

        /*UnityEngine.Debug.Log("tank is at high distance " + baseIsAtHighDistance);
        UnityEngine.Debug.Log("tank not behind " + baseAndTankIsNotBehindANotUs);
        UnityEngine.Debug.Log("e dist not too close " + enemyTankNotToCLoseToBase);*/
        return baseIsAtHighDistance && (enemyTankNotToCLoseToBase && baseAndTankIsNotBehindANotUs);

        // if  the enemy  tank isnt too close and the base isnt too close to retreat to then it is considered a viable saftey spot 

    }

    public void Update()
    {


        //UnityEngine.Debug.Log("saftey spot current pos " + safetySpot.transform.position);
        /*if (stateToReturn != null) // if we need to return a new state such as wait satet
        {
            UnityEngine.Debug.Log("trasnitioning to wait state ");

            transitionContext.GlobalObjectPositionForWait = Tank.LastKnownEPos;
            return stateToReturn; // return new state to be switched to 
        }*/

        if (Tank.enemyTank != null && retreatCheckComplete) // if we saw the enemy during the wait state 
        {
            hasPositionReference = true;
            retreatCheckComplete = false;
            //UnityEngine.Debug.Log("check complete tank seen ");
            bEnemySeen = true;
            hasPositionReference = true; /// new postition reference 
            t = runTime; // reset run time 
        }
        else if ((Tank.enemyTank == null) && (retreatCheckComplete)) // if we didnt see the enemy during the wait state(knwon through the transistion context)
        {

            //UnityEngine.Debug.Log("check complete tank not  seen returing to search ");

            //return typeof(SearchState);

        }


        getSafetySpot(); // calculate the safest point using the base position as default if we can(as it it not unknown)

        //UnityEngine.Debug.Log(safetySpot.transform.position);
        //UnityEngine.Debug.Log("does safety spot need to change " + changeSafteySpot);


        if ((safetySpot.transform.position == Tank.BasePositionStore || hasPositionReference)) // if we have a position reference to the enemy or we are going back to base 
        {
            //UnityEngine.Debug.Log("safety spot in if:" + safetySpot.transform.position);
            Tank.FollowPathToWorldPoint(safetySpot, fSpeed); //We go in the opposite direction of the enemy tank.
        }


       /* t -= Time.deltaTime; // decrement timer to look back againn
        UnityEngine.Debug.Log(" enemy seen decrment retreat time " + t);



        if (t <= 0.0f)// if runtime reaches 0 check for the enenmy 
        {
            UnityEngine.Debug.Log("runtime reached 0 state to returj set to wait");

            //stateToReturn = typeof(WaitState);

            //return null;
        }*/

        //return null;
    }


    private void getSafetySpot()
    {

        if (baseIsViable()) // check if the enemy tank is too close for us to retreat to base 
        {
            //UnityEngine.Debug.Log(Vector3.Distance(Tank.BasePositionStore, Tank.transform.position));

            safetySpot.transform.position = Tank.BasePositionStore; // if the base last known pos is viable to retreat to we use the base postion as the retareat spot as that is a known position
            /*   if (Tank.transform.position.z < 0.0f &&Tank.transform.position.x<0.0f )
               {
                   safetySpot.transform.position =new Vector3( Tank.transform.position.x*-1.0f,0.0f,0.0f);
               }*/

            //UnityEngine.Debug.Log("saftey spot set to base " + safetySpot.transform.position);


        }
        else // other wise we inverted the enemy tank position and recalaculate it if necessary 
        {

            //UnityEngine.Debug.Log("not safe to retreat to base ");
            if (!hasCalculatedEnemyInversion) // check if weve not already inverted the position so our saftey spot isnt ocnstantly changing when it does not need to 
            {
                findInversionToETank(Tank.LastKnownEPos.transform.position); // find inversion
                hasCalculatedEnemyInversion = true; // has inversion
            }

            if (isRetreatToNotSpotViable(Tank.transform.position, safetySpot.transform.position, retreatToInvertedBaseViableDistance)) // inverted spot no longer safe 
            {

                hasPositionReference = false;
                //UnityEngine.Debug.Log(hasPositionReference);
               // UnityEngine.Debug.Log("was not  viable to retreat to inverted enemy spot looking behind for new refernce to enemy");
                //stateToReturn = typeof(WaitState);

                findInversionToETank(Tank.LastKnownEPos.transform.position); // calculate new inversio 
                hasCalculatedEnemyInversion = true; // have calculated inversion
                t = runTime;
                /*                  UnityEngine.Debug.Log("does have reference " + hasPositionReference + " " + safetySpot.transform.position);
                */
                //UnityEngine.Debug.Log("reseting values retreat timer reset to: " + runTime + " enemy seen set to true: " + bEnemySeen);
                return;





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
        float directionToTravelZ = directionToTravel.z >= 0 ? 2.0f * retreatCheckDistance : -1.0f * (2.0f * retreatCheckDistance);

        safetySpot.transform.position = new Vector3(directionToTravelInX, 0, (directionToTravelZ));
        checkIfInCorner();
        //UnityEngine.Debug.Log("inverted spot " + safetySpot.transform.position);






    }

    private void checkIfInCorner()
    {
        Vector3 safetySpotCheck = Tank.transform.position + safetySpot.transform.position;

        bool checkGreaterZDir = safetySpotCheck.z > 0; // where the safety spot was placed realtive to enemy tank
        bool checkXGreaterpos = Tank.transform.position.x > 0;
        bool checkGreaterZpos = Tank.transform.position.z > 0;
        // if we are in a corner aleady we cant just take the enemy current facing direction and move towards it so we need to get out of the corner first 

        if (!checkXGreaterpos)// ensure that if we are in the top left or top right we dont chose a saftey spot behind us 
        {

            if (checkGreaterZDir && checkGreaterZpos) // if we are in the top left and we chose to go behind us
            {

                safetySpot.transform.position = new Vector3(Tank.transform.position.x, 0.0f, -Tank.transform.position.z);

            }
            else if (!checkGreaterZDir && !checkGreaterZpos)
            {
                safetySpot.transform.position = new Vector3(-Tank.transform.position.x, 0.0f, Tank.transform.position.z);

            }
            //UnityEngine.Debug.Log("had to adjust safety spot no longer using inverted z of tank  direction in x " + safetySpot.transform.position);

        }
        else if (checkXGreaterpos) // ensure that if we are in the bottom right or top right we dont chose a saftey spot behind us 
        {
            if (checkGreaterZDir && checkGreaterZpos) // if we are in the top left and we chose to go behind us
            {
                safetySpot.transform.position = new Vector3(-Tank.transform.position.x, 0.0f, Tank.transform.position.z); // go in a straight line from the inverted position 

            }
            else if (!checkGreaterZDir && !checkGreaterZpos)
            {
                safetySpot.transform.position = new Vector3(-Tank.transform.position.x, 0.0f, Tank.transform.position.z);


            }
            //UnityEngine.Debug.Log("had to adjust safety spot no longer using inverted z of tank  direction in x " + safetySpot.transform.position);

        }




        //UnityEngine.Debug.Log("inverted spot " + safetySpot.transform.position);



    }
    // is it safe to retreat to base 
    private bool isRetreatToNotSpotViable(Vector3 position, Vector3 positionOfRetreat, float viableDistance)
    {
        /*        || !Tank.compareDistanceBetwenPoints(safetySpot.transform.position, Tank.LastKnownEPos.transform.position);
         *        
        */
        changeSafteySpot = isToCloseToSafetySpotToRetreat(position, positionOfRetreat, viableDistance); // if we are too close to the position  we are retreating to 
        /* UnityEngine.Debug.Log("  change saftey spot " + Vector3.Distance(position,positionOfRetreat));
         if (!changeSafteySpot)
         {

             UnityEngine.Debug.Log(" is  safe to retreat to base ");
         }
         else
         {
             UnityEngine.Debug.Log("is not safe to retreat to base use inversion of e tank");

         }*/

        return changeSafteySpot;
    }
}
