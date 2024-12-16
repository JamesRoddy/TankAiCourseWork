using System;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;
public class CC_SearchState : BaseST
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
    bool hasFoundConsumable = false;
    int logCounter = 0;
    public CC_SearchState(CC_SmartTank newTank)
    {
        tank = newTank;

    }
    public override Type Entry()
    {
        currentSpeed = 0.85f;
        priorityPosition = new GameObject();
        stateToReturn = null;
        logCounter++;
        return null;
    }
    public override Type Exit()
    {
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
         if (tank.priorityManager.checkLow(PRIORITIES.FUEL))
        {
            currentSpeed = 0.6f;
        }
       

        if (tank.consumablesFound.Count > 0)
        {
            hasFoundConsumable = true;
            organiseConsumables();
            if (organisedConsumables.Count > 0) // if we saw any items 
            {
                EvaluatePriorityPositions();
            }
        }
        if (hasFoundConsumable)
        {
            currentSpeed = 1.0f;

        }

        if (priorityPositions.Count > 0 )
        {
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


    public void checkStateTransitions() // singular function that encapsulates all possible state transtions from search 
    {




        if (tank.enemyTank != null)
        {
            if (shouldRetreatFromSearch()) return;
            if (canAttackOrChaseETankFromSearch()) return;
            logCounter++;
        }
        else if (tank.enemyBase != null)
        {
            if (canAttackOrChaseEBaseFromSearch()) return;
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

        if (searchTimer > 15f && !tank.priorityManager.checkQueue(queuePriority.CRITICAL,PRIORITIES.AMMO)) // if we dont have crtical ammo we will transition into the ambush state to wait for the enemy 
            // the main reason fuek and health priority checks are omitted here is that ambush also acts asa stationary search state as by stopping and swivling the turret the ambush state
            // will also evalute the consumable it sees during this time allowing us to
            // sweep the area for any priority resources such as fuel and  health and will evalute which ones to go for based on their priorty(major minor critical,etc)
        {
            stateToReturn = typeof(CC_Ambush);
        }
        return stateToReturn != null;
    }


    private bool shouldRetreatFromSearch()
    {
        if ((tank.priorityManager.checkLow(PRIORITIES.HEALTH) || tank.priorityManager.checkQueue(queuePriority.CRITICAL,PRIORITIES.AMMO ) && tank.enemyTank!= null ))
            // we check the resource that are most critcal to an enagement that beng ammo and ehalth checking if ammo is not crticla(below half the lower thresh hold) as we still want to atake pop shots if we a re slightlty low on ammo 
            // we also check if we have enough health to take the enagment and if any of these condtions are not met we transition inot the retreat state to run away and potentialy find more counsumables in the process 
        {
            logCounter++;
            stateToReturn = typeof(CC_Retreat);
        }
        return stateToReturn != null;

    }

    private bool canAttackOrChaseEBaseFromSearch()
    {

        if (!tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO) && tank.enemyBase != null && tank.priorityManager.checkHigh(PRIORITIES.HEALTH))  // if our ammo is not of critcual priority we check further if we can enage the enemy base and we are high on health as shoooting the base makes us an easier target for the enemy tank
        {
            tank.TurretFaceWorldPoint(tank.enemyBase);
            if (tank.getDistanceToEnemyBase() < tank.BaseFiringDistance && tank.enemyBase != null) // if we are within our  firing distance so we dont miss the base
            {
                stateToReturn = typeof(CC_AttackState); // we attack the base and transition into the attack state
            }
            else if (tank.enemyBase != null && tank.getDistanceToEnemyBase() > tank.BaseFiringDistance) // other wise if we see the enemy base and are not in firing dist
            {
                stateToReturn = typeof(CC_Chase); // we apparoch the base to ensure the shot 
            }


        }

        return stateToReturn != null;


    }
    private bool canAttackOrChaseETankFromSearch()
    {
        if (tank.priorityManager.checkHigh(PRIORITIES.HEALTH) && !tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO)) // validate most critcla resources to attack
        {

            if (tank.getDistanceToEnemy() < tank.TankFiringDistance) // if we are within our firing disacnte to better ensure a shot 
            {
                stateToReturn = typeof(CC_AttackState); // transition to the attack state
            }
            else if (tank.priorityManager.checkHigh(PRIORITIES.FUEL)) // if we are not within our fiiring distance we return chase to get closer to ensure the shot on the enemy but only
                                                                      // if fuel is not of high prioirty i.e crtical or major ensuring that
                                                                      // we conserve fuel and dont chase as to not put our selevs at  a fuel
                                                                      // disadvantage  as to only get one for potential shot on the enemy 
            {
                stateToReturn = typeof(CC_Chase);
            }


        }


        return stateToReturn != null;
    }


    private void MoveToPriorityPositions()
    {

        if (priorityPositions.Count > 0 && tank.consumablesFound.Count>0) // if we find a reosucre in search it will be pushed to the priority resource list and if we find multiple coumsables at once they will be pushed in order of prioirty
        {

            priorityPosition.transform.position = priorityPositions[0];
            tank.FollowPathToWorldPoint(priorityPosition, currentSpeed);
            if (Vector3.Distance(priorityPosition.transform.position, tank.transform.position) < 5.0f) // move to the currrent priority consumable location
            {
                priorityPositions.RemoveAt(0);
            }


        }
        else // other wise we no longer see a consumable to we go back to moudlating our speed based on fuel be setting the has found conusmable bool to false
        {
            priorityPositions.Clear();
            hasFoundConsumable = false;
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
                    priorityPositions.Add(organisedConsumables[priority].transform.position);
                }
            }
        


    }





}