using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class CC_BTSearch : MonoBehaviour
{

    private CC_SmartTank tank;
    
    private List<GameObject> pointsOfInterest;
    float explorationTimer;
    float searchTimer;
    GameObject behind = new GameObject();
    private Type stateToReturn = null;
    List<Vector3> priorityPositions = new List<Vector3>();
    List<Vector3> visited;
    GameObject priorityPosition = new GameObject();
    public Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    private float currentSpeed = 0.85f;
    float checkBehindWaitTime = 0.0f;
    int logCounter = 0;


    public CC_BTSearch(CC_SmartTank newTank)
    {
        tank = newTank;
    }

    public void Update()

    {
        //If we are high on fuel we move at a faster speed
        if (tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            currentSpeed = 0.85f;

        }
        //Once we get low we slow down to conserve fuel. Which has helped us win several times against dumb tank.
        if (tank.priorityManager.checkLow(PRIORITIES.FUEL))
        {
            currentSpeed = 0.6f;
        }
        //Once we see a consumable we want to move to it at full speed to try and get it before our enemies.
        if (tank.consumablesFound.Count > 0)
        {
            currentSpeed = 1.0f;

        }

        if (tank.consumablesFound.Count > 0)
        {
            organiseConsumables();
            if (organisedConsumables.Count > 0) // if we saw any items 
            {
               

                EvaluatePriorityPositions();


            }
        }
        if (organisedConsumables.Count == 0)
        {

            priorityPositions.Clear();
        }

        if (priorityPositions.Count > 0)
        {



            MoveToPriorityPositions();

        }
        else
        {
            searching();
        }
        organisedConsumables.Clear();

    }

    public void organiseConsumables()
    {
        // form a dicitionary that catergorises  each resource currently in view 
        foreach (KeyValuePair<GameObject, float> gameObject in tank.consumablesFound) // loop through consumable dictionary 
        {
            if (!priorityPositions.Contains(gameObject.Key.transform.position))
            {
                if (gameObject.Key.CompareTag("Ammo")) // if the tag relates to the associated priority and the priority is not currenlty safe 
                {
                    organisedConsumables[PRIORITIES.AMMO] = gameObject.Key;// use the priority enum as the key and insert the found game object into the organisedConsumables dicitionary 

                }
                else if (gameObject.Key.CompareTag("Health"))
                {
                    organisedConsumables[PRIORITIES.HEALTH] = gameObject.Key;

                }
                else if (gameObject.Key.CompareTag("Fuel"))
                {
                    organisedConsumables[PRIORITIES.FUEL] = gameObject.Key;

                }

            }
        }

    }

    //We move to a random position at a set speed ,depending on our fuel, for 12 seconds.
    //After which we generate a new random point in the world and reset the timer.
    public void searching()
    {

        tank.FollowPathToRandomWorldPoint(currentSpeed);

        explorationTimer += Time.deltaTime;
        if (explorationTimer > 12.0f)
        {
            tank.GenerateNewRandomWorldPoint();
            explorationTimer = 0;
        }


    }

    public void MoveToPriorityPositions()
    {
        //We saw a pickup we store its position in a list of vectors and then go to that position.
        //To collect it.
        if (priorityPositions.Count > 0)
        {

            priorityPosition.transform.position = priorityPositions[0];
            tank.FollowPathToWorldPoint(priorityPosition, currentSpeed);
            if (Vector3.Distance(priorityPosition.transform.position, tank.transform.position) < 5.0f)
            {
                priorityPositions.RemoveAt(0); //Once we have collected the pickup we remove its position from the list to avoid going to that positon again.
            }


        }




    }

    public void EvaluatePriorityPositions()
    {

        bool isHealthMajor = tank.priorityManager.checkLow(PRIORITIES.HEALTH);
        bool isFuelMajor = tank.priorityManager.checkLow(PRIORITIES.FUEL); // check if current priority of fuel is low
        bool isAmmoMajor = tank.priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.AMMO);
        if (isFuelMajor || isHealthMajor) // if either was low
        {
            int getHealthOrFuel = Convert.ToInt32(organisedConsumables.ContainsKey(PRIORITIES.HEALTH)) - Convert.ToInt32(organisedConsumables.ContainsKey(PRIORITIES.FUEL));

            switch (getHealthOrFuel) // check boolean sum 
            {
                case -1:
                    {

                        priorityPositions.Add(organisedConsumables[PRIORITIES.FUEL].transform.position);  // asigning the current position of the found health pick up to a game object


                        break;
                    }
                case 1:
                    {

                        priorityPositions.Add(organisedConsumables[PRIORITIES.HEALTH].transform.position);
                        break;
                    }
                case 0:
                    {
                        if (organisedConsumables.ContainsKey(PRIORITIES.HEALTH))// if health was true that means a case of 0 means that both health and fuel were found in the orgainse items dictionary  
                        {

                            currentSpeed = tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.FUEL) ? currentSpeed : 0.65f; // in this case we also check if speed can be increased
                            Vector3 posHealth = organisedConsumables[PRIORITIES.HEALTH].transform.position;
                            Vector3 posFuel = organisedConsumables[PRIORITIES.FUEL].transform.position;
                            float distanceDifference = tank.consumablesFound[organisedConsumables[PRIORITIES.HEALTH]] - tank.consumablesFound[organisedConsumables[PRIORITIES.FUEL]];
                            if (distanceDifference < 0 && !tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.FUEL))
                            {
                                // check which pick up is closer if they are both major
                                // but in the situation where fuel is critcial it should alaways be pushed first to the priority points list
                                priorityPositions.Add(posHealth);
                                priorityPositions.Add(posFuel);


                            }
                            else
                            {
                                priorityPositions.Add(posFuel);
                                priorityPositions.Add(posHealth);


                            }


                        }

                        break;
                    }
            }



        }
        //after potentially pushing fuel and health to the priority list of postions check if ammo 
        //is crticial and if we have it in the  dicitionary meaning we have seen it push the game objects position to the priority positions list
        if (isAmmoMajor && organisedConsumables.ContainsKey(PRIORITIES.AMMO))
        {
            Vector3 ammoPos = organisedConsumables[PRIORITIES.AMMO].transform.position; ;
            priorityPositions.Add(ammoPos);


        }
        // sweep the remaining queues where resources would be of less of concern(minor/safe priority and see if any game resources were sighted that relate to that priority)
        foreach (PRIORITIES priority in tank.priorityManager.sweepQueues(new List<queuePriority> { queuePriority.MINOR, queuePriority.SAFE }))
        {
            if (organisedConsumables.ContainsKey(priority) && !priorityPositions.Contains(organisedConsumables[priority].transform.position))
            {
                priorityPositions.Add(organisedConsumables[priority].transform.position);
            }
        }


    }
}
