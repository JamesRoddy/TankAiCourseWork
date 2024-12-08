using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEditor.XR;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;
public class SearchState : BaseST
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
    Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    private float currentSpeed = 0.85f;
    float checkBehindWaitTime = 0.0f;
    int logCounter = 0;
    public SearchState(CC_SmartTank newTank)
    {
        tank = newTank;

    }
    public override Type Entry()
    {
        currentSpeed = 0.85f;
        priorityPosition = new GameObject();
        stateToReturn = null;
        Debug.Log("Entered Search " + logCounter);
        logCounter++;
        return null;
    }
    public override Type Exit()
    {
        Debug.Log("Search Exit " + logCounter);
        logCounter++;
        stateToReturn = null;
        priorityPositions.Clear();
        organisedConsumables.Clear();
        currentSpeed = 0.85f;
        explorationTimer = 0.0f;
        searchTimer = 0.0f;
        priorityPosition = new GameObject();

        return null;
    }
    public override Type Update()
    {
        checkStateTransitions();
        if (stateToReturn != null)
        {

            return stateToReturn;

        }
        if (tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            currentSpeed = 0.85f;


        }
        else if (tank.priorityManager.checkLow(PRIORITIES.FUEL))
        {
            currentSpeed = 0.5f;
        }

        if (tank.consumablesFound.Count > 0)
        {
            organiseConsumables();
            if (organisedConsumables.Count > 0) // if we saw any items 
            {
                Debug.Log("number of consumables found " + organisedConsumables.Count);

                EvaluatePriorityPositions();


            }
        }
        if (tank.WasHit && tank.enemyTank == null)
        {
            behind.transform.position = new Vector3(tank.transform.position.x, 0, -tank.transform.forward.z);
            tank.stopAndCheckPos(behind, 2.0f, tank.enemyTank, ref checkBehindWaitTime);

            Debug.Log("tank checking behind as was hit in search and enemy tank is null");

            return null;

        }

        if (priorityPositions.Count > 0)
        {
            currentSpeed = 0.95f;
            if (organisedConsumables.Count == 0)
            {
                priorityPositions.Clear();
            }
            MoveToPriorityPositions();
        }
        else
        {
            searching();
        }
        organisedConsumables.Clear();

        return null;

    }




    private void organiseConsumables()
    {
        // form a dicitionary that catergorises  each resource currently in view 
        Debug.Log("consumables reset " + organisedConsumables.Count);

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



    private void searching()
    {


        tank.FollowPathToRandomWorldPoint(currentSpeed);

        explorationTimer += Time.deltaTime;
        if (explorationTimer > 12.0f)
        {
            tank.GenerateNewRandomWorldPoint();
            explorationTimer = 0;
        }





    }


    public void checkStateTransitions()
    {




        if (tank.enemyTank != null)
        {
            //Debug.Log("would exit");
            Debug.Log("Seen Enemey Tank");
            if (shouldRetreatFromSearch()) return;
            Debug.Log("no switch to retreat from search");
            if (canAttackOrChaseETankFromSearch()) return;
            Debug.Log("no switch to attack from search due to ammo");
            logCounter++;
            Debug.Log("No Transition from search");



        }
        else if (tank.enemyBase != null)
        {
            Debug.Log("Seen Enemey base");

            if (canAttackOrChaseEBaseFromSearch()) return;
            Debug.Log("No Transition from search");


        }


        else
        {
            if (shouldStartCamping()) return;
        }

        stateToReturn = null;


    }

    private bool shouldStartCamping()
    {
        searchTimer += Time.deltaTime;

        if (searchTimer > 15f)
        {
            stateToReturn = typeof(Ambush);
        }
        return stateToReturn != null;
    }


    private bool shouldRetreatFromSearch()
    {
        if (tank.priorityManager.checkLow(PRIORITIES.HEALTH) && tank.enemyBase == null)
        {
            Debug.Log("search switch to retreat low health " + logCounter);
            logCounter++;
            stateToReturn = typeof(Retreat);
        }
        return stateToReturn != null;

    }

    private bool canAttackOrChaseEBaseFromSearch()
    {

        Debug.Log("enemy base null in search " + (tank.enemyBase == null));

        Debug.Log("Seen Enemy Base");
        if (!tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO) && tank.enemyBase != null)
        {
            //chase
            Debug.Log("search switch to attack base  ammo not crticial " + logCounter);
            tank.TurretFaceWorldPoint(tank.enemyBase);
            if (tank.getDistanceToEnemyBase() < tank.BaseFiringDistance && tank.enemyBase != null)
            {
                Debug.Log(tank.getDistanceToEnemyBase());
                Debug.Log("  search switch to attack in firing distance  " + tank.BaseFiringDistance + " " + logCounter);
                stateToReturn = typeof(CC_AttackState);
            }
            else if (tank.enemyBase != null && tank.getDistanceToEnemyBase() > tank.BaseFiringDistance)
            {

                Debug.Log(" search switch to chase not in firing distance  " + tank.BaseFiringDistance + " " + logCounter);
                stateToReturn = typeof(Chase);
            }
            logCounter++;


        }

        return stateToReturn != null;


    }
    private bool canAttackOrChaseETankFromSearch()
    {
        if (tank.priorityManager.checkHigh(PRIORITIES.HEALTH) && !tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
        {

            if (tank.getDistanceToEnemy() < tank.TankFiringDistance)
            {
                Debug.Log(tank.getDistanceToEnemy());
                Debug.Log("  search switch to attack in firing distance  " + tank.TankFiringDistance + " " + logCounter);
                stateToReturn = typeof(CC_AttackState);
            }
            else if (tank.priorityManager.checkHigh(PRIORITIES.FUEL))
            {

                Debug.Log(" search switch to chase not in firing distance and high fuel " + tank.TankFiringDistance + " " + logCounter);
                stateToReturn = typeof(Chase);
            }


        }


        return stateToReturn != null;
    }

    private void rushConsumable()
    {

        /*            List<PriorityManager.queuePriority> queuesToSweep = new List<PriorityManager.queuePriority>{PriorityManager.queuePriority.MAJOR,PriorityManager.queuePriority.MINOR };
            List<PRIORITIES> currentPriorites = tank.priorityManager.sweepQueues(queuesToSweep);

            if (organisedConsumables.Count > 0 && !tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL,PRIORITIES.HEALTH))
            {
                GameObject consumableToRush = null;

                foreach (PRIORITIES priorities in currentPriorites)
                {
                    if (organisedConsumables.ContainsKey(priorities) ) {

                        consumableToRush = organisedConsumables[priorities];
                        break;                      

                    }


                }







            }*/

    }
    private void MoveToPriorityPositions()
    {

        if (priorityPositions.Count > 0)
        {

            priorityPosition.transform.position = priorityPositions[0];
            tank.FollowPathToWorldPoint(priorityPosition, currentSpeed);
            Debug.Log("moving to priority position " + priorityPosition.transform.position);
            Debug.Log(Vector3.Distance(tank.transform.position, priorityPosition.transform.position));
            if (Vector3.Distance(priorityPosition.transform.position, tank.transform.position) < 5.0f)
            {
                Debug.Log("removed position " + priorityPosition.transform.position);
                priorityPositions.RemoveAt(0);
            }


        }




    }
    private void EvaluatePriorityPositions()
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
        /// after potentially pushing fuel and health to the priority list of postions check if ammo 
        /// is crticial and if we have it in the  dicitionary meaning we have seen it push the game objects position to the priority positions list
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
                Debug.Log("added minor/safe priority " + priority + " in to priority position list");
                priorityPositions.Add(organisedConsumables[priority].transform.position);
            }
        }


    }





}