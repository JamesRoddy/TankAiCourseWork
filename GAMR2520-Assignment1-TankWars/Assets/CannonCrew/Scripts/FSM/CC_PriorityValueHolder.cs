using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// wrapper class for values associated with priorites(ammo,health,fuel)
public class PriorityValuesHolder 
{

    private float normalisedMinThresh; // define min percentage thresh hold
    private float normalisedMaxThresh; // define max
    private float normalisedCurrentValue; // current value updated by smart tank
    
    public PriorityValuesHolder(float lowThreshHold, float highThreshHold, float threshHoldivider )
    {
        Debug.Log("non normalised threshHolds " + lowThreshHold + " " + highThreshHold + " max: " + threshHoldivider);
        normalisedMinThresh = lowThreshHold / threshHoldivider;
        normalisedMaxThresh = highThreshHold / threshHoldivider;
        Debug.Log("normalisedThreshHoldlow " + normalisedMinThresh);
        Debug.Log("normalisedThreshHoldhigh " + normalisedMaxThresh);

    }

    public float CurrentPriorityVal
    {
        get { return normalisedCurrentValue; }
        set { normalisedCurrentValue = value; }
    
    }
    public float LowerThreshHold
    {

        get { return normalisedMinThresh; }

    }
    public float HigherThreshHold
    {

        get { return normalisedMaxThresh; }

    }







}
