using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor.XR;
using UnityEngine;
using static CC_SmartTank;

public class SearchState : BaseST
{

    private CC_SmartTank tank;
    private List<GameObject> pointsOfInterest;
    float explorationTimer;
    private Type stateToReturn = null;
    List<Vector3> priorityPositions = new List<Vector3>();
    List<Vector3> visited;
    Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    private float currentSpeed;
    public SearchState(CC_SmartTank newTank )
    {
        tank = newTank;
        
    }
    public override Type Entry()
    {
        
        
        return null;
    }
    public override Type Exit()
    {
        stateToReturn = null;
        priorityPositions.Clear();
        organisedConsumables.Clear();
        currentSpeed = 0.85f;
        explorationTimer = 0.0f;
        return null;
    }
    public override Type Update()
    {

       if (checkStateTransitions())
       {

            return stateToReturn;

       }
        if (tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
            currentSpeed = 0.85f;
           

        }
        else if(tank.priorityManager.checkLow(PRIORITIES.FUEL)) 
        {
            currentSpeed = 0.4f;
        }

        if (tank.consumablesFound.Count > 0)
        {
            // form a dicitionary that catergorises  each resource currently in view 
            organisedConsumables = new Dictionary<PRIORITIES, GameObject> ();

            foreach(KeyValuePair<GameObject,float> gameObject in tank.consumablesFound) // loop through consumable dictionary 
            {
                if (gameObject.Key.CompareTag("Ammo") && !tank.priorityManager.isResourceSafe(PRIORITIES.AMMO)) // if the tag relates to the associated priority and the priority is not currenlty safe 
                {
                    organisedConsumables[PRIORITIES.AMMO] = gameObject.Key;// use the priority enum as the key and insert the found game object into the organisedConsumables dicitionary 

                }
                else if (gameObject.Key.CompareTag("Health") && !tank.priorityManager.isResourceSafe(PRIORITIES.HEALTH))
                {
                    organisedConsumables[PRIORITIES.HEALTH] = gameObject.Key;

                }
                else if (gameObject.Key.CompareTag("Fuel") && !tank.priorityManager.isResourceSafe(PRIORITIES.FUEL))
                {
                    organisedConsumables[PRIORITIES.FUEL] = gameObject.Key;

                }

            }

            if (organisedConsumables.Count>0) // if we saw any items 
            {

                EvaluatePriorityPositions();


                // potential to do(take into account minor priorites when searching)
                /*if (tank.priorityManager.hasItems(PriorityManager.queuePriority.MINOR))
                {
                    

                }*/

            }



        }
        if (priorityPositions.Count > 0) {
            MoveToPriorityPositions();
        }

        searching();
        return null;
    }




    private void searching()
    {
        if (!checkStateTransitions())
        {
            tank.FollowPathToRandomWorldPoint(currentSpeed);

            explorationTimer += Time.deltaTime;

            if (explorationTimer > 12.0f)
            {
                tank.GenerateNewRandomWorldPoint();
                explorationTimer = 0;
            }

        }
      


    }


    public bool checkStateTransitions()
    {


        if(tank.enemyTank != null)
        {
            Debug.Log("would exit");

            if (tank.priorityManager.checkLow(PRIORITIES.HEALTH))
            {

                stateToReturn = typeof(Retreat);
            }
            if( tank.priorityManager.checkHigh(PRIORITIES.HEALTH) 
                && tank.priorityManager.checkHigh(PRIORITIES.HEALTH) 
                && !tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
                //chase
                //stateToReturn = typeof(chaseState);

            }
            return false;


        }
        return false;







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
    private void MoveToPriorityPositions() {

          int count = 0;
          GameObject target = new GameObject();
          while (count < priorityPositions.Count) 
          {
             if (checkStateTransitions()) break;

             target.transform.position = priorityPositions[count];
             tank.FollowPathToWorldPoint(target,currentSpeed);
             if(Vector3.Distance(tank.transform.position, target.transform.position) <= 1.0f)
             {
                count++;
             }
             
            
          }
          priorityPositions.Clear();



        
    
    }
    private void EvaluatePriorityPositions ()
    {

        bool isHealthMajor = tank.priorityManager.checkLow(PRIORITIES.HEALTH);
        bool isFuelMajor = tank.priorityManager.checkLow(PRIORITIES.FUEL); // check if current priority of fuel is low
        bool isAmmoCritcial = tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL, PRIORITIES.AMMO);
        if (isFuelMajor || isHealthMajor) // if either was low
        {
            int getHealthOrFuel = Convert.ToInt32(organisedConsumables.ContainsKey(PRIORITIES.HEALTH)) - Convert.ToInt32(organisedConsumables.ContainsKey(PRIORITIES.FUEL));

            switch (getHealthOrFuel) // check boolean sum 
            {
                case -1:
                    {

                        priorityPositions.Add( organisedConsumables[PRIORITIES.HEALTH].transform.position);  // asigning the current position of the found health pick up to a game object


                        break;
                    }
                case 1:
                    {

                        priorityPositions.Add( organisedConsumables[PRIORITIES.FUEL].transform.position);
                        break;
                    }
                case 0:
                    {
                    if (organisedConsumables.ContainsKey(PRIORITIES.HEALTH))// if health was true that means a case of 0 means that both health and fuel were found in the orgainse items dictionary  
                    { 

                       currentSpeed = tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL, PRIORITIES.FUEL) ? currentSpeed : 0.65f; // in this case we also check if speed can be increased
                       Vector3 posHealth = organisedConsumables[PRIORITIES.HEALTH].transform.position;
                       Vector3 posFuel = organisedConsumables[PRIORITIES.FUEL].transform.position;
                       float distanceDifference = tank.consumablesFound[organisedConsumables[PRIORITIES.HEALTH]] - tank.consumablesFound[organisedConsumables[PRIORITIES.FUEL]];
                       if (distanceDifference < 0 && !tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL,PRIORITIES.FUEL)) 
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
           /// after potentially pushing fuel and health to the priority list of postions check if ammo 
           /// is crticial and if we have it in the  dicitionary meaning we have seen it push the game objects position to the priority positions list
            if (isAmmoCritcial && organisedConsumables.ContainsKey(PRIORITIES.AMMO)) 
            {
                Vector3 ammoPos = organisedConsumables[PRIORITIES.AMMO].transform.position; ;
                priorityPositions.Add(ammoPos);


            }







        }


    }
    public void checkFuelSpeed() 
    {




        currentSpeed = tank.priorityManager.checkQueue(PriorityManager.queuePriority.CRITICAL, PRIORITIES.FUEL) ? currentSpeed : 0.65f; // if our priority for fuel is major and not cirticla it means that we should have enough to increase our speed to reach the found fuel faster

    }




}
