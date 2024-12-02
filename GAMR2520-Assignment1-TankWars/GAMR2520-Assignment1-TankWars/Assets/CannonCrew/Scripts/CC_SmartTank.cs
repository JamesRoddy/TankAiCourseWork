using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AStar;

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

    public Dictionary<GameObject, float> enemyTanksFound = new Dictionary<GameObject, float>();     /*!< <c>enemyTanksFound</c> stores all tanks that are visible within the tanks sensor. */
    public Dictionary<GameObject, float> consumablesFound = new Dictionary<GameObject, float>();    /*!< <c>consumablesFound</c> stores all consumables that are visible within the tanks sensor. */
    public Dictionary<GameObject, float> enemyBasesFound = new Dictionary<GameObject, float>();     /*!< <c>enemyBasesFound</c> stores all enemybases that are visible within the tanks sensor. */
    float t;    /*!< <c>t</c> stores timer value */
    public HeuristicMode heuristicMode; /*!< <c>heuristicMode</c> Which heuristic used for find path. */

    // enums for prioirity these can be obtained through prioritites.name 
    public enum PRIORITIES
    {
        HEALTH,
        AMMO,
        FUEL,
        NONE,

    }

    private void initStateMachine()
    {

        Dictionary<Type,BaseST> states = new Dictionary<Type,BaseST>();


        GetComponent<CC_FSM>().setStates(states);


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

    /// <summary>
    /// Generate a path from current position to pointInWorld (GameObject). If no heuristic mode is set, default is Euclidean,
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    public void GeneratePathToWorldPoint(GameObject pointInWorld)
    {
        a_FindPathToPoint(pointInWorld);
    }

    /// <summary>
    /// Generate a path from current position to pointInWorld (GameObject). Using a defined heuristic mode.
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    /// <param name="heuristic">Chosen heuristic for path finding</param>
    public void GeneratePathToWorldPoint(GameObject pointInWorld, HeuristicMode heuristic)
    {
        a_FindPathToPoint(pointInWorld, heuristic);
    }

    /// <summary>
    ///Generate and Follow path to pointInWorld (GameObject) at normalizedSpeed (0-1). If no heuristic mode is set, default is Euclidean,
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    /// <param name="normalizedSpeed">This is speed the tank should go at. Normalised speed between 0f,1f.</param>
    public void FollowPathToWorldPoint(GameObject pointInWorld, float normalizedSpeed)
    {
        a_FollowPathToPoint(pointInWorld, normalizedSpeed);
    }

    /// <summary>
    ///Generate and Follow path to pointInWorld (GameObject) at normalizedSpeed (0-1). 
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    /// <param name="normalizedSpeed">This is speed the tank should go at. Normalised speed between 0f,1f.</param>
    /// <param name="heuristic">Chosen heuristic for path finding</param>
    public void FollowPathToWorldPoint(GameObject pointInWorld, float normalizedSpeed, HeuristicMode heuristic)
    {
        a_FollowPathToPoint(pointInWorld, normalizedSpeed, heuristic);
    }

    /// <summary>
    ///Generate and Follow path to a randome point at normalizedSpeed (0-1). Go to a randon spot in the playfield. 
    ///If no heuristic mode is set, default is Euclidean,
    /// </summary>
    /// <param name="normalizedSpeed">This is speed the tank should go at. Normalised speed between 0f,1f.</param>
    public void FollowPathToRandomWorldPoint(float normalizedSpeed)
    {
        a_FollowPathToRandomPoint(normalizedSpeed);
    }

    /// <summary>
    ///Generate and Follow path to a randome point at normalizedSpeed (0-1). Go to a randon spot in the playfield
    /// </summary>
    /// <param name="normalizedSpeed">This is speed the tank should go at. Normalised speed between 0f,1f.</param>
    /// <param name="heuristic">Chosen heuristic for path finding</param>
    public void FollowPathToRandomWorldPoint(float normalizedSpeed, HeuristicMode heuristic)
    {
        a_FollowPathToRandomPoint(normalizedSpeed, heuristic);
    }

    /// <summary>
    ///Generate new random point
    /// </summary>
    public void GenerateNewRandomWorldPoint()
    {
        a_GenerateRandomPoint();
    }

    /// <summary>
    /// Stop Tank at current position.
    /// </summary>
    public void TankStop()
    {
        a_StopTank();
    }

    /// <summary>
    /// Continue Tank movement at last know speed and pointInWorld path.
    /// </summary>
    public void TankGo()
    {
        a_StartTank();
    }

    /// <summary>
    /// Face turret to pointInWorld (GameObject)
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    public void TurretFaceWorldPoint(GameObject pointInWorld)
    {
        a_FaceTurretToPoint(pointInWorld);
    }

    /// <summary>
    /// Reset turret to forward facing position
    /// </summary>
    public void TurretReset()
    {
        a_ResetTurret();
    }

    /// <summary>
    /// Face turret to pointInWorld (GameObject) and fire (has delay).
    /// </summary>
    /// <param name="pointInWorld">This is a gameobject that is in the scene.</param>
    public void TurretFireAtPoint(GameObject pointInWorld)
    {
        a_FireAtPoint(pointInWorld);
    }

    /// <summary>
    /// Returns true if the tank is currently in the process of firing.
    /// </summary>
    public bool TankIsFiring()
    {
        return a_IsFiring;
    }

    /// <summary>
    /// Returns float value of remaining health.
    /// </summary>
    /// <returns>Current health.</returns>
    public float TankCurrentHealth
    {
        get
        {
            return a_GetHealthLevel;
        }
    }

    /// <summary>
    /// Returns float value of remaining ammo.
    /// </summary>
    /// <returns>Current ammo.</returns>
    public float TankCurrentAmmo
    {
        get
        {
            return a_GetAmmoLevel;
        }
    }

    /// <summary>
    /// Returns float value of remaining fuel.
    /// </summary>
    /// <returns>Current fuel level.</returns>
    public float TankCurrentFuel
    {
        get
        {
            return a_GetFuelLevel;
        }
    }

    /// <summary>
    /// Returns list of friendly bases. Does not include bases which have been destroyed.
    /// </summary>
    /// <returns>List of your own bases which are. </returns>
    protected List<GameObject> MyBases
    {
        get
        {
            return a_GetMyBases;
        }
    }

    /// <summary>
    /// Returns Dictionary(GameObject target, float distance) of visible targets (tanks in TankMain LayerMask).
    /// </summary>
    /// <returns>All enemy tanks currently visible.</returns>
    protected Dictionary<GameObject, float> VisibleEnemyTanks
    {
        get
        {
            return a_TanksFound;
        }
    }

    /// <summary>
    /// Returns Dictionary(GameObject consumable, float distance) of visible consumables (consumables in Consumable LayerMask).
    /// </summary>
    /// <returns>All consumables currently visible.</returns>
    protected Dictionary<GameObject, float> VisibleConsumables
    {
        get
        {
            return a_ConsumablesFound;
        }
    }

    /// <summary>
    /// Returns Dictionary(GameObject base, float distance) of visible enemy bases (bases in Base LayerMask).
    /// </summary>
    /// <returns>All enemy bases currently visible.</returns>
    protected Dictionary<GameObject, float> VisibleEnemyBases
    {
        get
        {
            return a_BasesFound;
        }
    }

}
