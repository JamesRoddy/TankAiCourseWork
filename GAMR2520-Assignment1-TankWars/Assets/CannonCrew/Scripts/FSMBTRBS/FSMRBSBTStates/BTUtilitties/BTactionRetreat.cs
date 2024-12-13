using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// wrapper for all actions the behaviour tree can perform when in retreat state
public class BTactionRetreat : MonoBehaviour
{
    CC_smartTankFSMRBSBT Tank;
    float runTime = 8.5f;
    private bool hasCalculatedEnemyInversion = false;
    float runtimeIncrement = 0.0f;
    float tankCheckBehindTime = 1.0f;
    float waitTime = 0.0f;
    float retreatToSpotDist = 15.0f;
    bool firstSight = false;
    bool hasinversion = false;
    float currentSpeed = 1.0f;
    private float retreatCheckDistance = 35.0f;
    private float retreatToBaseViableDistance = 90.0f;
    private GameObject safetySpot = new GameObject();

    public BTactionRetreat(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
        
    }





    public bool checkEtankPos()
    {
        bool waiting;

        if (runtimeIncrement > runTime || CheckPositionReference())
        {
            waiting = Tank.stopAndCheckPos(Tank.LastKnownEPos, tankCheckBehindTime, Tank.enemyTank, ref waitTime);
            Debug.Log("waiting for enemy retreat");
            if (waiting)
            {
                runtimeIncrement = 0.0f;
                Debug.Log("set runtimer back to 0");


            }
            return waiting;
        }
        else
        {
            Debug.Log("returned true automatically retreat");

            return true;
        }


    }




    private void findInversionToETank(Vector3 pos)
    {

        Vector3 directionToTravel = Vector3.Normalize(Tank.transform.position - pos);
        float directionToTravelInX = directionToTravel.x >= 0 ? 1.5f * retreatCheckDistance : -1.0f * (1.5f * retreatCheckDistance);
        float directionToTravelZ = directionToTravel.z >= 0 ? 2.0f * retreatCheckDistance : -1.0f * (2.0f * retreatCheckDistance);

        safetySpot.transform.position = new Vector3(directionToTravelInX, 0, (directionToTravelZ));
        checkIfInCorner();
        UnityEngine.Debug.Log("inverted spot " + safetySpot.transform.position);






    }

    public bool findSafteySpot()
    {


        if (baseIsViable())
        {
            safetySpot.transform.position = Tank.BasePositionStore;
            hasinversion = false;
            Debug.Log("base was viable");

        }
        else
        {


            if (Tank.enemyTank == null)
            {

                Debug.Log("enemy tank was null when getting pos reference");
                return false;
            }

            if (!hasCalculatedEnemyInversion)
            {
                Debug.Log("calculating enemy tank inversion");
                hasinversion = true;
                findInversionToETank(Tank.LastKnownEPos.transform.position);

            }

            if (CheckPositionReference())
            {
                Debug.Log("need position refence no longer has inversion");

                hasinversion = false;
                return false;
            }





        }
        Debug.Log("saftey spoot found success");

        return true;




    }

    public bool CheckPositionReference()
    {

        if (isToCloseToSafetySpotToRetreat(Tank.transform.position, safetySpot.transform.position, retreatToSpotDist))
        {
            Debug.Log("to close to saftey spot ");


            return true;


        }
        return false;
    }

    public bool running()
    {


        if (runtimeIncrement < runTime)
        {
            runtimeIncrement += Time.deltaTime;
            Debug.Log("run increment time " + runtimeIncrement);
            Tank.FollowPathToWorldPoint(safetySpot, currentSpeed);




        }

        return runtimeIncrement >= runTime;


    }



    private bool isRetreatToNotSpotViable(Vector3 position, Vector3 positionOfRetreat, float viableDistance)
    {

        return isToCloseToSafetySpotToRetreat(position, positionOfRetreat, viableDistance);

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
            UnityEngine.Debug.Log("had to adjust safety spot no longer using inverted z of tank  direction in x " + safetySpot.transform.position);

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
            UnityEngine.Debug.Log("had to adjust safety spot no longer using inverted z of tank  direction in x " + safetySpot.transform.position);

        }




        UnityEngine.Debug.Log("inverted spot " + safetySpot.transform.position);



    }






    private bool isToCloseToSafetySpotToRetreat(Vector3 positionOfTank, Vector3 positionOfRetreat, float viableDistance)
    {
        return Vector3.Distance(positionOfTank, positionOfRetreat) <= viableDistance;
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

        UnityEngine.Debug.Log("tank is at high distance " + baseIsAtHighDistance);
        UnityEngine.Debug.Log("tank not behind " + baseAndTankIsNotBehindANotUs);
        UnityEngine.Debug.Log("e dist not too close " + enemyTankNotToCLoseToBase);
        return baseIsAtHighDistance && (enemyTankNotToCLoseToBase && baseAndTankIsNotBehindANotUs);

        // if  the enemy  tank isnt too close and the base isnt too close to retreat to then it is considered a viable saftey spot 

    }
}
