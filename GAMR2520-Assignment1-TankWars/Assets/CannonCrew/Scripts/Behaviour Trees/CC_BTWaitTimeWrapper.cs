using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_BTWaitTimeWrapper : MonoBehaviour
{

    public float waitTime = 0.0f; // wait time for how lonmg the tnak should look in the deifned direction
    public int logCounter;
    public GameObject objectToLookFor = new GameObject(); // what we are looking for 
    public GameObject objectPosition = new GameObject();// the position to look at
    public float waitTimeRef = 0.0f;// timer to increment
    public bool wasInRetreat = false;


}

   
