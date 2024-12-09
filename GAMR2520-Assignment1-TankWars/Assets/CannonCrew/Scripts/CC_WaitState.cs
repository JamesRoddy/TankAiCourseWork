using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitState : BaseST
{



    private BaseAIBehaviourModel transitionContext; // get the gloabl timers used to set the context of the wait state 
    // so if we are switching from retreat to wait for example we will be looking for the enemy tank for a certain amount of time before moving 
    // therefore the context of the wait must be known where as with the ambush state we are still looking for the enemy tank but for a certain amount of time 
    private float waitTime = 0.0f; // wait time for how lonmg the tnak should look in the deifned direction
    private int logCounter;
    private GameObject objectToLookFor = new GameObject(); // what we are looking for 
    private GameObject objectPosition =  new GameObject();// the position to look at
    private float waitTimeRef = 0.0f;// timer to increment
    private CC_SmartTank tank;
    private bool wasInRetreat = false;
    public WaitState(BaseAIBehaviourModel behaviourModel,CC_SmartTank tank)
    {
        
        transitionContext = behaviourModel;
        this.tank = tank;
    }

    public override Type Entry()
    {
        Debug.Log("wait entered "+logCounter);
        logCounter++;
        waitTime = transitionContext.GlobalTimerForWaitState; // get the context for the current wait state based on state that transitioned into it 
        objectPosition = transitionContext.GlobalObjectPositionForWait; // get the position we are looking at during the wait 
        wasInRetreat = transitionContext.wasInState(typeof(Retreat)); // if our previous state was the retreat

        return null;
    }

    public override Type Exit()
    {
        Debug.Log("wait exit " + logCounter);
        logCounter++;
        waitTimeRef = 0.0f;
        return null;
    }


    public override Type Update()
    {
        Debug.Log("wait update");

       
        if (wasInRetreat) // if we were in the retreat state
        {
            Debug.Log("getting reference to enemy tank");
            objectToLookFor = tank.enemyTank; // set and update the appropriate object to look for during wait
        }


        if (tank.stopAndCheckPos(objectPosition, waitTime,objectToLookFor , ref waitTimeRef)){ // make the tank look ata positio  for a certain amount of time 
            
            Debug.Log("waiting for "+waitTime+" current time "+waitTimeRef);
            Debug.Log( "was retreating " + (transitionContext.PreviousBehaviourStateType ==  typeof(Retreat)));
            return transitionContext.PreviousBehaviourStateType; // jump back to previous behaviour that returned the wait state 
            
        };


        return null;
    }



}
