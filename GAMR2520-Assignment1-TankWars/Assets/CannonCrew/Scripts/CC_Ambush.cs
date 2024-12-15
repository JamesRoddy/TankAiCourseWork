using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using static PriorityManager;

public class Ambush : BaseST
{
    GameObject lastKnownConsumablePos = new GameObject();
    CC_SmartTank Tank;
    float fTimeLimit = 15.0f;
    float t;
    float fRotate;
    bool hasChecked;
    BaseAIBehaviourModel transitionContext;
    GameObject tankPosition = new GameObject();
    GameObject orbitPath = new GameObject();
    GameObject origin = new GameObject();
    PRIORITIES resourceFoundDuringAmbush = PRIORITIES.NONE;
    bool hasFoundConsumable = false;
    GameObject consumableFound = new GameObject();
    int logCounter;
    bool waitCheckComplete;
    float angleIncrement;
    //This state is for when we havent seen anything important for 15 seconds.
    //So what it does is stop the tank, saving our precious fuel, and constantly rotate the turret in circles for 15 seconds lying in wait to ambush.
    //Once the enemy tank comes into proximity we ATTACK unless on of our resources is far too low.
    public Ambush(CC_SmartTank newtank, BaseAIBehaviourModel transitionContext)
    {
        Tank = newtank;
        this.transitionContext = transitionContext;
    }
    public override Type Entry()
    {
        Debug.Log("Entered Camping");
        waitCheckComplete = transitionContext.wasInState(typeof(WaitState));

        if (!waitCheckComplete) // only reset the turret if we wouldnt be looking at a consumable
        {
            hasFoundConsumable = false;
            Tank.TurretReset();
        }

        hasFoundConsumable = false;
        lastKnownConsumablePos.transform.position = Vector3.zero;
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = Vector3.zero;
        origin.transform.position = Vector3.zero;

        fRotate = 0.0f;
        t = 0.0f;
        return null;
    }
    public override Type Update()
    {

        Tank.TankStop();
        t += Time.deltaTime;

        if (waitCheckComplete) // if we waited fully to see if the consumable was still there(this will only trigger if the consuamble was of high priroity interupting the ambush 
        {
            Debug.Log("ambush found prioirity resource wait check complete moving to search");
            return typeof(SearchState);
        }
        if ((t <= fTimeLimit && !waitCheckComplete))
        {


            if (Tank.enemyTank != null && Tank.priorityManager.checkHigh(PRIORITIES.HEALTH)) // if we are still wainting and see the enemy tank and we are on high healthn we go into the attack styate to get the drop on the enemy while we are stood still(fire first)
            {
                return typeof(CC_AttackState);
            }

            hasFoundConsumable = consumableFound.transform.position != Vector3.zero; // checking for consumables 
            if (!hasFoundConsumable || (resourceFoundDuringAmbush != PRIORITIES.NONE && !Tank.priorityManager.checkLow(resourceFoundDuringAmbush)
                && Tank.priorityManager.checkLow(Tank.resourceFoundWhileWaiting))) // if we havent found a consuamble or see one of higher priority while we are wiaitng for the enemy tank
            {
                consumableFound.transform.position = Tank.checkConsumablesWhileWaiting().transform.position;
                resourceFoundDuringAmbush = Tank.resourceFoundWhileWaiting; // we will get the resource found during ambush 
                Debug.Log("consumables found during ambush " + consumableFound.transform.position);

            }




            if (!checkTimeLimitToMoveToConsumable() && Tank.enemyTank != null && Tank.priorityManager.checkLow(PRIORITIES.HEALTH))  //if the enemy tank interupted us looking around but we were low on health
            {
                float dotBetweenEnenmyAndResouce = Vector3.Dot(Vector3.Normalize(consumableFound.transform.position - Tank.transform.position), Tank.EtankLastKnownTransformForward); // see if the resource is behind us 

                if (resourceFoundDuringAmbush != PRIORITIES.HEALTH || (resourceFoundDuringAmbush == PRIORITIES.HEALTH && !(dotBetweenEnenmyAndResouce > 0))) // if the resouce was health but in front of us ie near the enemy tank or it was not health
                {
                    return typeof(Retreat); // we go into retreat from ambush
                }
                t = fTimeLimit; // other wise we immidealy set our ambush timer to the max so we finish it 

            }
            if (checkTimeLimitToMoveToConsumable()) // if the time limit is at 30% and weve found a consumable of high prioiryt during ambush we dont wait as long for the enemy tank and move to the conusmable instead
            {


                transitionContext.SetWaitStateGlobalContext(consumableFound, 2.5f, false); // set the context for the wait state before going into it
                return typeof(WaitState);
            }
            fRotate += Time.deltaTime;
            //Rotates the turret over a period of time
            //Need to find a way to adjust the speed.

            orbitPath.transform.position = new Vector3(Mathf.Sin(fRotate) * (Time.realtimeSinceStartup * 10.0f), 0.0f, Mathf.Cos(fRotate) * (Time.realtimeSinceStartup * 10.0f));
            tankPosition.transform.position = tankPosition.transform.position + orbitPath.transform.position;

            Tank.TurretFaceWorldPoint(tankPosition);
            return null;

        }

        if (Tank.enemyTank != null && Tank.priorityManager.checkHigh(PRIORITIES.HEALTH))
        {
            return typeof(CC_AttackState);
        }
        if (hasFoundConsumable && !waitCheckComplete)// if we found a consumable and havent already waited to check if its there 
        {
            Debug.Log("should wait in ambush ");
            transitionContext.SetWaitStateGlobalContext(consumableFound, 2.5f, false);
            return typeof(WaitState); // jump out of ambush state to get any consumables we found that may not be of priority but we still need them  
        }


        hasFoundConsumable = false;
        return typeof(SearchState);
    }





    private bool checkTimeLimitToMoveToConsumable()
    {
        if (t > fTimeLimit * 0.3 && hasFoundConsumable)
        {
            if (Tank.priorityManager.checkLow(resourceFoundDuringAmbush)) // if we had any resources of high priority
            {

                // we dont know the position of the object as the ambush state will repetedly turn the turret for
                // 30% of its original wait time event when it sees a consumable it needs
                // so we may not have eyes on the consumable initially so we need to turn the turret to look at it before we can move to it 
                return true;
            }
        }

        return false;
    }
    private void checkForConsumable()
    {


        lastKnownConsumablePos.transform.position = Tank.checkConsumablesWhileWaiting().transform.position;
        // get the consumable we saw if we saw multiple it will be one of highest priroity
        if (lastKnownConsumablePos.transform.position != Vector3.zero)
        { // found consumable 
            hasFoundConsumable = true;
        }




    }

    public override Type Exit()
    {
        Debug.Log("Exited Camping");
        orbitPath.transform.position = Vector3.zero;
        tankPosition.transform.position = Vector3.zero;
        origin.transform.position = Vector3.zero;
        Tank.TurretReset();
        lastKnownConsumablePos.transform.position = Vector3.zero;
        fRotate = 0.0f;
        t = 0.0f;
        return null;
    }


}

