using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_BTactionRetreat : MonoBehaviour
{
    CC_smartTankFSMRBSBT Tank;
    float runTime = 8.5f;
    private bool hasCalculatedEnemyInversion = false;
    float runtimeIncrement = 0.0f;
    float tankCheckBehindTime = 1.0f;
    float waitTime = 0.0f;
    float retreatToSpotDist = 15.0f;
    bool haseWaited = false; 

    bool firstSight = false;
    bool hasinversion = false;
    float currentSpeed = 1.0f;
    bool enemyTankNull;
    private float retreatCheckDistance = 35.0f;
    private float retreatToBaseViableDistance = 90.0f;
    private GameObject safetySpot = new GameObject();

    public  CC_BTactionRetreat(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
        
    }





    public bool checkEtankPos()
    {
        Debug.Log("waiting");

       
        if (runtimeIncrement >= runTime || CheckPositionReference() ) // if we need a postiion refernce or if the runtime is exceeded 
        {
            Debug.Log(runtimeIncrement + "after run");
            Debug.Log("needed position reference" + CheckPositionReference());
            
            if (Tank.stopAndCheckPos(Tank.LastKnownEPos, tankCheckBehindTime, Tank.enemyTank, ref waitTime)) // look back for a reference to the enemy
            {
                Debug.Log("returned true wait retreat");
                if (Tank.enemyTank != null) {
                    Debug.Log("returned true wait retreat");
                    
                    resetTimers();
                    Debug.Log("runtime is " + runtimeIncrement);

                }

            
                return true;


            };
            Debug.Log("returned false wait retreat");

            return false;

        }
        Debug.Log("no condtion was hit for wiating");
        return true;



    }


    public bool hasFinishedRun()
    {
        return runtimeIncrement >= runTime;
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

        if (baseIsViable()) // check if the base is a valid coation for the tank to move to 
        {
            safetySpot.transform.position = Tank.BasePositionStore; // set the saftey spot to the base 
            hasinversion = false; // we dont need to use the enemies inverted position at this point as we know the base is safe 
            Tank.FollowPathToWorldPoint(safetySpot, currentSpeed);
            return true;
        }
        else
        {


            if (!hasinversion && Tank.stats["enemySeen"]) // if we dont have a current postion refence to the enemy and we see them
            {
                hasinversion = true; // set the inversion
                findInversionToETank(Tank.enemyTank.transform.position); // calculate the inverison
                Tank.FollowPathToWorldPoint(safetySpot, currentSpeed); // move to the location considered safe 

            }
            else if (CheckPositionReference() ) // if we dont have a position reference to the enemy
            {
                

                hasinversion = false; // we no longer have a refernce to a spot that would be safe
                return false;
            }
            Debug.Log("saftey spoot inversion found success");
            Tank.FollowPathToWorldPoint(safetySpot, currentSpeed); // follow the path to the saftey spot if we succed in fidning one 
            return true;


        }


    }

    public bool CheckPositionReference() // used to validate the sfaety spot chosen by the tank
    {
       
        if (isToCloseToSafetySpotToRetreat(Tank.transform.position, safetySpot.transform.position, retreatToSpotDist) )
        {
            Debug.Log("to close to saftey spot ");
           

            return true;


        }
        return false;
    }

    public bool running()
    {


        if (runtimeIncrement < runTime) // coniute runnign to the saftey spot based ona timer so that we conver a certain amount of distance before looking behind us 
        {
            runtimeIncrement += Time.deltaTime;
            Debug.Log("run increment time " + runtimeIncrement);
            Tank.FollowPathToWorldPoint(safetySpot, currentSpeed);


        }

        return runtimeIncrement < runTime;


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
                UnityEngine.Debug.Log("had to adjust safety spot no x was - z was + " + safetySpot.transform.position);

            }
            else if (!checkGreaterZDir && !checkGreaterZpos)
            {
                safetySpot.transform.position = new Vector3(-Tank.transform.position.x, 0.0f, Tank.transform.position.z);
                UnityEngine.Debug.Log("had to adjust safety spot no x was - z was - " + safetySpot.transform.position);

            }

        }
        else if (checkXGreaterpos) // ensure that if we are in the bottom right or top right we dont chose a saftey spot behind us 
        {
            if (checkGreaterZDir && checkGreaterZpos) // if we are in the top left and we chose to go behind us
            {
                safetySpot.transform.position = new Vector3(-Tank.transform.position.x, 0.0f, Tank.transform.position.z); // go in a straight line from the inverted position 
                UnityEngine.Debug.Log("had to adjust safety spot no x was + z was +" + safetySpot.transform.position);

            }
            else if (!checkGreaterZDir && !checkGreaterZpos)
            {
                safetySpot.transform.position = new Vector3(-Tank.transform.position.x, 0.0f, Tank.transform.position.z);
                UnityEngine.Debug.Log("had to adjust safety spot no x was + z was -" + safetySpot.transform.position);


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


        // check that we wont double back into the enemy tank if we chose the aftey spot as our base location
        float dotBetweenUsAndEnemy = Vector3.Dot(Tank.transform.forward, Tank.EtankLastKnownTransformForward);
        float dotBetweenBaseDirAndUs = Vector3.Dot(Tank.transform.forward, Vector3.Normalize(Tank.transform.position - Tank.BasePositionStore));
        bool baseAndTankIsNotBehindANotUs = !(dotBetweenUsAndEnemy >= 0 && dotBetweenBaseDirAndUs >= 0);
        // check the enemy tank isnt too close to the base 
        float distanceToBaseOfEnemyTank = Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.BasePositionStore);
        // check that we are not to close to the base
        bool baseIsAtHighDistance = Vector3.Distance(Tank.transform.position, Tank.BasePositionStore) > retreatToBaseViableDistance;
        bool enemyTankNotToCLoseToBase = Tank.compareDistanceBetwenPoints(Tank.BasePositionStore, Tank.LastKnownEPos.transform.position);
        float distanceToBaseAndEnemy = Vector3.Distance(Tank.LastKnownEPos.transform.position, Tank.BasePositionStore);
        // if  the enemy  tank isnt too close and the base isnt too close to retreat to then it is considered a viable saftey spot 


        return baseIsAtHighDistance && (enemyTankNotToCLoseToBase && baseAndTankIsNotBehindANotUs);


    }


   public void resetTimers() // reset timers for retreat actions 
    {
        waitTime = 0;
        runtimeIncrement = 0.0f;

        hasinversion = false;
        safetySpot.transform.position = Tank.BasePositionStore;
    }
}
