using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PriorityManager;

public class BTActionsSearch : MonoBehaviour
{



    CC_smartTankFSMRBSBT Tank;
    GameObject currentPriorityPosition;
    private float currentSpeed = 0.85f;
    private float maxSearchTime = 12.0f;
    float explorationTimer;
    GameObject consumablePosition;
    Dictionary<PRIORITIES,GameObject> organisedConsumables = new Dictionary<PRIORITIES,GameObject>();
    public BTActionsSearch(CC_smartTankFSMRBSBT tank)
    {
        this.Tank = tank;
    }

    public void findPriorityResource(PRIORITIES currentPriorityResource)
    {


        organiseConsumables();

        if (organisedConsumables.ContainsKey(currentPriorityResource) ) {

            currentPriorityPosition.transform.position = organisedConsumables[currentPriorityResource].transform.position;
        
        }
        else
        {
            currentPriorityPosition.transform.position = -organisedConsumables[currentPriorityResource].transform.position;
        }



        

    }

   
    
    public void moveToConsumable()
    {
        

        if (!(Vector3.Distance(Tank.transform.position, currentPriorityPosition.transform.position) < 5.0f))
        {
            Tank.FollowPathToWorldPoint(currentPriorityPosition,currentSpeed);
            return;

        }
        if (Tank.consumablesFound.First().Key != null)
        {
            if (!(Vector3.Distance(Tank.transform.position, consumablePosition.transform.position) < 5.0f))
            {
                Tank.FollowPathToWorldPoint(Tank.consumablesFound.First().Key, currentSpeed);
               

            }



        }




    }

    
    



    public void search()
    {


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
        // form a dicitionary that catergorises  each resource currently in view 
        /*        Debug.Log("consumables reset " + organisedConsumables.Count);
        */
        foreach (KeyValuePair<GameObject, float> gameObject in Tank.consumablesFound) // loop through consumable dictionary 
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
