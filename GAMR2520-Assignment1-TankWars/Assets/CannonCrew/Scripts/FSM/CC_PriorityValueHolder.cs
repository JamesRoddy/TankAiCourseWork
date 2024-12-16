using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// wrapper class for values associated with priorites(ammo,health,fuel)
public class PriorityValuesHolder 
{

    private float normalisedMinThresh; // define min percentage thresh hold
    private float normalisedMaxThresh; // define max
    private float normalisedCurrentValue; // current value updated by smart tank
    
    public PriorityValuesHolder(float lowThreshHold, float highThreshHold, float max )
    {
        
        normalisedMinThresh = lowThreshHold / max;
        normalisedMaxThresh = highThreshHold / max;
        

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
