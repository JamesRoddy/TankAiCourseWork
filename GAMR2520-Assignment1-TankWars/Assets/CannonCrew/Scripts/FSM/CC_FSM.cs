using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CC_FSM:BaseAIBehaviourModel
{

    

    Dictionary<Type, BaseST> states; // dicitionary that will store all of the states available to the
   // state machine with the key for a particualr state being a type associated with one of
   // the child classes of the abstract  base class 
    BaseST currentState; // the current state of the state machine 
  
    public BaseST CState // getter and setter for current state
    {
        get
        {
            return currentState;
        }
        private set
        {

            currentState = value;
        }

    } 

    
    public void setStates(Dictionary<Type, BaseST> newStates)
    {
        
        states = newStates;
    }

    private void Update()
    {

        if (CState == null) // if our state machine is running for the first time(we dont have a state)
        {
            CState = states.First().Value; // set the current state to the default sate 
            Debug.Log("init BTFSMRBS"+CState.GetType());
        }
        else
        {
            var next = currentState.Update(); // continuely update the current state 

            if (next != null && next.GetType() != CState.GetType()) // waiting for change in state i.e none rull return from update  
            {
                switchState(next); // change currentState to next state 
            }
        }


    }



    private void switchState(Type next)
    {

        CState.Exit();// when switching states we call the exit function for the current state before switching 
        PreviousBehaviour = CState; // define previous state 

        Debug.Log("set previous state "+( PreviousBehaviourStateType == CState.GetType()));
        Debug.Log("previous state == wait " + (CState.GetType()  ));
        CState = states[next];//  key into the states dictionary in order to get the state being transitioned to using the type of the next state


        CState.Entry(); // set up the state that has juts been transitioned to 
        


    }




}
