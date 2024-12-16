using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PriorityManager;
// wrapper for all actions the behaviour tree can perform when in search state

public class CC_BTActionsSearch : MonoBehaviour
{



    CC_smartTankFSMRBSBT Tank;
    public GameObject currentPriorityPosition;
    public float currentSpeed = 0.85f;
    private float consumableCheckDist = 5.0f;
    private float maxSearchTime = 12.0f;
    public int priorityResourceNotHitCounter = 0;
    public int priorityResourceNoHitMax = 3;
    public bool hasFoundResource = false;
    public bool hasFoundPriorityResource = false;
    float explorationTimer; 
    
    public GameObject consumablePosition;

    Dictionary<PRIORITIES, GameObject> organisedConsumables = new Dictionary<PRIORITIES, GameObject>();
    public CC_BTActionsSearch(CC_smartTankFSMRBSBT tank)
    {
        this.Tank = tank;
        currentPriorityPosition = new GameObject();
        consumablePosition = new GameObject();
    }

    public bool findPriorityResource(PRIORITIES currentPriorityResource)
    {


        organiseConsumables();
        if (organisedConsumables.Count > 0)
        {
                    if (organisedConsumables.ContainsKey(currentPriorityResource)) { // if we see are priroity resource 

            currentPriorityPosition.transform.position = organisedConsumables[currentPriorityResource].transform.position; // move towards its position based on distance 


        }
        else
        {
            currentPriorityPosition.transform.position = -organisedConsumables[currentPriorityResource].transform.position; // other wise we invert its position to go search for what we need 
        }
        }



        if(priorityResourceNoHitMax == priorityResourceNotHitCounter) // if we have not  hit our priority resource for a certain number of times we will take the reosurce infront of us ensuring that we dont fall behind on other resources types that might not be the highest priority at the moment 
        {
            return true;
        }

        if (!(Vector3.Distance(Tank.transform.position, currentPriorityPosition.transform.position) < consumableCheckDist)) // check if weve reached the desired consumable
        {

            Tank.FollowPathToWorldPoint(currentPriorityPosition, currentSpeed);
            return false;

        }


        return true;

    }



    // utlitlty method sto reset various vairbales associated with the actions related with search
    public bool isAtPriorityNoResourceHitMax()
    {
        return priorityResourceNotHitCounter == priorityResourceNoHitMax;
    }
    public void resetPriorityResourceNotFoundCounter()
    {
        
        priorityResourceNotHitCounter = 0;
    }
    public void incrementPriorityResourceNotFoundCounter()
    {
        priorityResourceNotHitCounter++;
    }
    public void checkIfPriorityResource(PRIORITIES priorityResource) 
    {
        organiseConsumables(); /// categorise resources 
        if (!organisedConsumables.ContainsKey(priorityResource))//f wthe conusmable we see isnt the priority resource we need 
        {

            priorityResourceNoHitMax++; // inrement the counter each time we dont hit the prioriyt resource we need 

        }
    }


    public bool moveToConsumable()
    {


        if (Tank.consumablesFound.Count>0 && Tank.consumablesFound.First().Key != null) // if we see a consumable a
        {
            currentSpeed = 1.0f;
            consumablePosition.transform.position = Tank.consumablesFound.First().Key.transform.position; // gte its postion and move to it based on distance 
            if (priorityResourceNotHitCounter >= priorityResourceNoHitMax) // if we are taking this consumable based on us not being able to find the priority resource
            {
                currentPriorityPosition.transform.position = -consumablePosition.transform.position;// still take its innverted psotion ready to find the priority resource again
            }
            if (!(Vector3.Distance(Tank.transform.position, consumablePosition.transform.position) < consumableCheckDist))
            {

                Tank.FollowPathToWorldPoint(Tank.consumablesFound.First().Key, currentSpeed);
                return false;

            }
        }
        currentSpeed = 0.85f;
        return true;

    }

    
    public void search()
    {
        if (Tank.stats["lowFuel"]) // modulate speed based on fuel priority
        {
            currentSpeed = 0.6f;
        }
        else
        {
            currentSpeed = 0.85f;
        }
        Tank.FollowPathToRandomWorldPoint(currentSpeed);

        explorationTimer += Time.deltaTime; 
        if (explorationTimer > maxSearchTime)
        {
            Tank.GenerateNewRandomWorldPoint();
            explorationTimer = 0;
        }





    }





    private void organiseConsumables()
    {
        
        
        foreach (KeyValuePair<GameObject, float> gameObject in Tank.consumablesFound) // loop through consumable dictionary 
        {
              // catgorise and organise resources 
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
