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
public class CC_SearchStateRBS : BaseST
{

    private CC_SmartTankRBS Tank;
    private List<GameObject> pointsOfInterest;
    float explorationTimer;
    GameObject behind = new GameObject();
    bool hasFoundConsuamble = false;
    private Type stateToReturn = null;
    List<Vector3> priorityPositions = new List<Vector3>();
    List<Vector3> visited;
    GameObject priorityPosition = new GameObject();
    Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    private float currentSpeed = 0.85f;
    float checkBehindWaitTime = 0.0f;
    int logCounter = 0;
    public CC_SearchStateRBS(CC_SmartTankRBS newTank)
    {
        Tank = newTank;

    }
    public override Type Entry()
    {
      //  currentSpeed = 0.85f;
        priorityPosition = new GameObject();
        stateToReturn = null;
        Debug.Log("Entered Search " + logCounter);
        Tank.resetTimersIntoSearch(); // reset timers coming in from search
        Tank.stats["searchState"] = true; // search state true

        logCounter++;
        return null;
    }
    public override Type Exit()
    {
        Debug.Log("Search Exit " + logCounter);

        Tank.stats["searchState"] = false;
        hasFoundConsuamble = false;
        logCounter++;
        stateToReturn = null;
        priorityPositions.Clear();
        organisedConsumables.Clear();
      //  currentSpeed = 0.85f;
        explorationTimer = 0.0f;
        priorityPosition = new GameObject();

        return null;
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
        Debug.Log("current speed " + currentSpeed);
        if (Tank.priorityManager.checkHigh(PRIORITIES.FUEL))
        {
         //   currentSpeed = 0.85f;


        }
        else if (Tank.stats["lowFuel"] == true)
        {
         //   currentSpeed = 0.5f;
        }
      

        if (Tank.consumablesFound.Count > 0)
        {
            if (!hasFoundConsuamble)
            {
                hasFoundConsuamble = true;
            }
            organiseConsumables();
            if (organisedConsumables.Count > 0) // if we saw any items 
            {
/*                Debug.Log("number of consumables found " + organisedConsumables.Count);
*/
                EvaluatePriorityPositions();


            }
        }
        if (hasFoundConsuamble == true)
        {
            currentSpeed = 1.0f;
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

        return null;

    }




    private void organiseConsumables()
    {
        // form a dicitionary that catergorises  each resource currently in view 
        foreach (KeyValuePair<GameObject, float> gameObject in Tank.consumablesFound) // loop through consumable dictionary 
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


        Tank.FollowPathToRandomWorldPoint(currentSpeed);

        explorationTimer += Time.deltaTime;
        if (explorationTimer > 12.0f)
        {
            Tank.GenerateNewRandomWorldPoint();
            explorationTimer = 0;
        }





    }
    private void MoveToPriorityPositions()
    {

        if (priorityPositions.Count > 0 &&  Tank.consumablesFound.Count > 0)
        {

            priorityPosition.transform.position = priorityPositions[0];
                Tank.FollowPathToWorldPoint(priorityPosition, currentSpeed);
            Debug.Log("moving to priority position " + priorityPosition.transform.position);
            Debug.Log(Vector3.Distance(Tank.transform.position, priorityPosition.transform.position));
            if (Vector3.Distance(priorityPosition.transform.position, Tank.transform.position) < 5.0f)
            {
                Debug.Log("removed position " + priorityPosition.transform.position);
                priorityPositions.RemoveAt(0);
            }


        }
        else
        {
            priorityPositions.Clear();
            hasFoundConsuamble = false;
        }



    }
    private void EvaluatePriorityPositions()
    {
        if (Tank.stats["lowHealth"] == true || Tank.stats["lowFuel"] == true) // if either was low
        {
            int getHealthOrFuel = Convert.ToInt32(organisedConsumables.ContainsKey(PRIORITIES.HEALTH)) - Convert.ToInt32(organisedConsumables.ContainsKey(PRIORITIES.FUEL));
            
            switch (getHealthOrFuel) // check boolean sum 
            {
                case -1:
                    {
                        Debug.Log("going for fuel ");
                        priorityPositions.Add(organisedConsumables[PRIORITIES.FUEL].transform.position);  // asigning the current position of the found health pick up to a game object


                        break;
                    }
                case 1:
                    {
                        Debug.Log("going for health ");
                        priorityPositions.Add(organisedConsumables[PRIORITIES.HEALTH].transform.position);
                        break;
                    }
                case 0:
                    {

                        Debug.Log("going for health and fuel ");

                        if (organisedConsumables.ContainsKey(PRIORITIES.HEALTH))// if health was true that means a case of 0 means that both health and fuel were found in the orgainse items dictionary  
                        {
                            Vector3 posHealth = organisedConsumables[PRIORITIES.HEALTH].transform.position;
                            Vector3 posFuel = organisedConsumables[PRIORITIES.FUEL].transform.position;
                            float distanceDifference = Tank.consumablesFound[organisedConsumables[PRIORITIES.HEALTH]] - Tank.consumablesFound[organisedConsumables[PRIORITIES.FUEL]];
                            if (distanceDifference < 0 && !Tank.priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.FUEL))
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
        if (Tank.stats["ammoMajor"] && organisedConsumables.ContainsKey(PRIORITIES.AMMO))
        {
            Vector3 ammoPos = organisedConsumables[PRIORITIES.AMMO].transform.position; ;
            priorityPositions.Add(ammoPos);


        }
        // sweep the remaining queues where resources would be of less of concern(minor/safe priority and see if any game resources were sighted that relate to that priority)
        
        
            foreach (PRIORITIES priority in Tank.priorityManager.sweepQueues(new List<queuePriority> { queuePriority.MINOR, queuePriority.SAFE }))
            {
                if (organisedConsumables.ContainsKey(priority) && !priorityPositions.Contains(organisedConsumables[priority].transform.position))
                {
                    Debug.Log("added minor/safe priority " + priority + " in to priority position list");
                    priorityPositions.Add(organisedConsumables[priority].transform.position);
                }
            }
        



    }





}
