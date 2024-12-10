using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;
using static PriorityManager;

 public class PriorityHolder // each priority holder is repsobile for managing a single resource such as ammo or fuel 

{ 
    PriorityValuesHolder priorityValues; // the wrapper class that stores and keep track of values from smart tank
    private PriorityManager.queuePriority state;// current priroity queue that this priority holder is in 
    private PriorityManager.queuePriority previousState; // previous queue 
    private PRIORITIES priorityName; // name of resource associated with priority holder 
    public PriorityHolder(PRIORITIES newPriorityName, PriorityManager.queuePriority newState, PriorityValuesHolder valuesAssociatedWithPriority)
    {
           priorityValues = valuesAssociatedWithPriority;
           state = newState;
           previousState = newState;
           priorityName = newPriorityName;
          
    }


    public bool checkSafe() // check if the current resource is not below a defined safety thresh hold or at max
    {
     
       
        if(state != PriorityManager.queuePriority.SAFE && CurrentValue >= SafetyThreshHold) // if we are not safe but are above the saftey thresh hold 
        {
            Debug.Log("is safe now ");
            PreviousClassification = CurrentClassification; // set previous and current state back to default(safe)
            CurrentClassification = PriorityManager.queuePriority.SAFE;
       
        }
        
        return CurrentValue >= SafetyThreshHold; // return true if above or equal to defined saftey threshold 
    }

    public void setSafe() // set current state and prev to safe 
    {
        CurrentClassification = PriorityManager.queuePriority.SAFE;
        PreviousClassification = CurrentClassification;
    }
    // asside from just the safe state the method below  is a
    // wrapper for all methods that manage other priorities
    // and will return if there has been a change in state for this priority
    public bool checkForHigherPriorites()  
    {
        // set previous state to current in case of change of state below 
        setPreviousState();
        checkMinor();
        checkMajor();
    

        return PreviousClassification != CurrentClassification; // return if there has been a change in priority 

    }
    public void checkMajor()
    {
        
            // check for what would transition us out of this state 
            if (CurrentValue < PriorityThreshHold / 2.0f)
            {
                CurrentClassification = PriorityManager.queuePriority.CRITICAL;


            }

            if (CurrentValue > PriorityThreshHold && CurrentValue < SafetyThreshHold)
            {
                CurrentClassification = PriorityManager.queuePriority.MINOR;


            }
        


    }
    public void setPreviousState()
    {
        PreviousClassification = CurrentClassification;

    }
    public void checkMinor()

    {
        // 
        if(CurrentValue>PriorityThreshHold && CurrentValue < SafetyThreshHold )
        {
            CurrentClassification = PriorityManager.queuePriority.MINOR;

            
        }

        if (CurrentValue < PriorityThreshHold)
        {
            CurrentClassification = PriorityManager.queuePriority.MAJOR;


        }



    }


    public PRIORITIES Name
    {
        get { return priorityName; }
    }
    public float PriorityThreshHold
    {
           get { return priorityValues.LowerThreshHold; }
    }

    public float SafetyThreshHold
    {
        get { return priorityValues.HigherThreshHold; }
    }
    public float CurrentValue
    {
            get{ return priorityValues.CurrentPriorityVal; }
    }
    public PriorityManager.queuePriority PreviousClassification
    {
        private set { previousState = value; }
        get { return previousState; }
      
    }
    public PriorityManager.queuePriority CurrentClassification
    {
            get{ return state; }
            private set{ state = value; }
    }

 };

