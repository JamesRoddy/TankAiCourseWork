using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;


// this class is used to define infomration for any global ehaviours between two different ai systems such as FSM and behaviour trees 

// for example the wait state will cause the tank to stop and look in a particualr direction 
// but this depends on context and therefore the state will need to be aware of the previous that transitioned into it 
// this also allows for dynamic adjustment of the timers before going inot the wait state based on previous state
// making code easier to reuse for global behvaiours beween different AI techniques (such as the wait state)
public class BaseAIBehaviourModel : MonoBehaviour
{
    // when the tank enters the wait state it will be looking for a particualr game object so we have
    // our context for why we are waiting(state we transitioned from) and the current time/object to use for that context


    protected BaseST previousState; // defines the context (allows this to be adjusted per behaviour used without needing to know which script is attached to the game object using the behavour )
    private GameObject toLookForInWait;
    private GameObject posToLookForInWait;
    bool waitShouldCheckForObject;
    private float waitTimer = 0.0f;
    private void Awake()
    {
        UnityEngine.Debug.Log("base ai behaviour awake");
        toLookForInWait = new GameObject();
        posToLookForInWait = new GameObject();

    }

    public Type PreviousBehaviourStateType
    {

        get { return previousState.GetType(); }

    }

    protected BaseST PreviousBehaviour
    {
        set { previousState = value; }
    }

    public void SetWaitStateGlobalContext(GameObject positionToLookAt,  float waitTime,bool isCheckingForObject)
    {
        toLookForInWait = positionToLookAt;

        waitShouldCheckForObject = isCheckingForObject;

        UnityEngine.Debug.Log(posToLookForInWait.transform.position);
        waitTimer = waitTime;   
    }     

    // allow access to previous state allowing certain states such as the wait state to have context provided to them such as how  wait or the object tp look for based on previous state 
    public bool wasInState(Type stateType)
    {
        UnityEngine.Debug.Log("was in previous state of " + stateType);
        return PreviousBehaviourStateType == stateType;
    }

    public bool WaitCheckingForObject
    {
        get { return waitShouldCheckForObject; }
    }
    public GameObject GlobalObjectPositionForWait
    {
        set { posToLookForInWait = value; }
        get { return posToLookForInWait; }
    }
    public float GlobalTimerForWaitState
    {
        set { waitTimer = value; }
        get { return waitTimer; }

     }
}
