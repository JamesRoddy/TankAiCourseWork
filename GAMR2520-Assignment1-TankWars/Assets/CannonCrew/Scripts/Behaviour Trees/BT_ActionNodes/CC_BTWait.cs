using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_BTWait : MonoBehaviour
{

    private float waitTime = 0.0f; // wait time for how lonmg the tnak should look in the deifned direction
    private int logCounter;
    private GameObject objectToLookFor = new GameObject(); // what we are looking for 
    private GameObject objectPosition = new GameObject();// the position to look at
    private float waitTimeRef = 0.0f;// timer to increment
    private CC_SmartTank tank;
    private bool wasInRetreat = true;

    public CC_BTWait(CC_SmartTank newtank)
    {
        tank = newtank;
    }

   
            
        
    


    // Update is called once per frame
    public void Update()
    {
        
        if (wasInRetreat) // if we were in the retreat state
        {
            waitTime = 2f;
            waitTimeRef = 0.5f;
            objectPosition = tank.LastKnownEPos;
            Debug.Log("in wait" + objectPosition.transform.position);
            Debug.Log("getting reference to enemy tank");
            objectToLookFor = tank.enemyTank; // set and update the appropriate object to look for during wait
        }

        //tank.stopAndCheckPos(objectPosition, waitTime, objectToLookFor, ref waitTimeRef);

    }
}
