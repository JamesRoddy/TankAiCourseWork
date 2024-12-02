using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC_SmartTank : AITank
{
    // taking into account the extra health ammo and fuel that can be collected(when caclulating the maxiumum for each resource)
    private float ammoMaxOffset = 5.0f;
    private float fuelMaxOffset = 25.0f;
    private float healthMaxOffset = 25.0f;

    /*    // current percentages for tank resources
        private float healthPercentage;
        private float fuelPercentage;
        private float ammoPercentage;*/

    // maximum for each resource
    private float maxHealth;
    private float maxAmmo;
    private float maxFuel;

    // default thresholds for resources becoming a priority 
    float healthPriorityThresh;
    float ammoPriorityThresh;
    float fuelPriorityThresh;

    // keep track of the current targets for the tank
    public GameObject consumable;
    public GameObject enemyTank;
    public GameObject enemyBase;
    public PRIORITIES currentPriority;
    // enums for prioirity these can be obtained through prioritites.name 
    public enum PRIORITIES
    {
        HEALTH,
        AMMO,
        FUEL,
        NONE,

    }

  
    // idea: priority queue(might become more relevant as project moves on )
    /*   public List<PRIORITIES> currentPriorites = new List<PRIORITIES> {   };*/

    public override void AITankStart()
    {

        // calc maxiumum for resources 
        maxHealth = a_GetHealthLevel;
        maxAmmo = a_GetAmmoLevel + ammoMaxOffset;
        maxFuel = a_GetFuelLevel;
        currentPriority = PRIORITIES.NONE;
        // current thresholds for when something should become a priority
        healthPriorityThresh = 30.0f;
        ammoPriorityThresh = 4.0f;
        fuelPriorityThresh = 40.0f;




    }
    public override void AIOnCollisionEnter(Collision collision)
    {

    }
    public override void AITankUpdate()
    {

        /*setPriority();*/


    }
    // methods for checking if a specifc resource is low 
    private bool CheckLowHealth()
    {

        return a_GetHealthLevel <= healthPriorityThresh;

    }
    private bool CheckLowAmmo()
    {
        return a_GetAmmoLevel <= ammoPriorityThresh;



    }
    private bool CheckLowFuel()
    {
        return a_GetFuelLevel <= fuelPriorityThresh;



    }
    // check if we have any low resources at all 
    private bool hasLowResource()
    {

        int isLow = Convert.ToInt32(CheckLowFuel()) + Convert.ToInt32(CheckLowAmmo()) + Convert.ToInt32(CheckLowHealth());


        return isLow > 0;


    }
    // get lowest resource will return the HEALTH enum if low health for example 
    public PRIORITIES GetCurrentLowestResource()
    {
        if (hasLowResource()) // if we have any resource that is below its thresh hold
        {
            float min = Mathf.Min(a_GetAmmoLevel / maxAmmo, Mathf.Min(a_GetHealthLevel / maxHealth, a_GetFuelLevel / maxFuel)); // get the lowest percentage out of the resources


            // return the current highest priority resource based on it being the lowest in terms of amount 
            if (min == a_GetHealthLevel / maxHealth)
            {
                return PRIORITIES.HEALTH;
            }
            else if (min == a_GetFuelLevel / maxAmmo)
            {
                return PRIORITIES.FUEL;
            }

            return PRIORITIES.AMMO;

        }


        return PRIORITIES.NONE;// if we didnt have any resource of priority return none 

    }


    // allows for manual assignment of currentPrioity
    public PRIORITIES setCurrentPriority{

        set
        {
            currentPriority = value;
        }

      }

 
    public void setPriorityToLowest()
    {

            currentPriority = GetCurrentLowestResource();
         
    }



   
}
