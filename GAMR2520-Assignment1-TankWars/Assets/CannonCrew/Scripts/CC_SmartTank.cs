using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static AStar;
using static CC_SmartTank;
using static PriorityManager;




public class CC_SmartTank : AITank
{
    // taking into account the extra health ammo and fuel that can be collected(when caclulating the maxiumum for each resource)
    private float ammoMaxOffset = 5.0f;
    private float fuelMaxOffset = 25.0f;
    private float healthMaxOffset = 25.0f;
    private float tankFiringDistance = 40.0f;
    private float enemyBaseFiringDistance = 30.0f;
    protected bool wasHit = false;
    public PriorityValuesHolder healthValuesHolder;
    public PriorityValuesHolder fuelValuesHolder;
    public PriorityValuesHolder ammoValuesHolder;
    private Type stateToReturn = null;
    public PriorityManager priorityManager;
    private Vector3 currentSafteySpot;



    // current percentages for tank resources
    protected float healthPercentage;
    protected float fuelPercentage;
    protected float ammoPercentage;




    // maximum for each resource
    private float maxHealth;
    private float maxAmmo;
    private float maxFuel;

    private float hitTimer = 1.5f; // usedto set us being hit to false after certain amount of time

    // default thresholds for resources becoming a priority 
    public float healthPriorityThresh;
    public float ammoPriorityThresh;
    public float fuelPriorityThresh;
    public float healthSafteyThresh;
    public float ammoSafteyThresh;
    public float fuelSafteyThresh;



    // keep track of the current targets for the tank
    public GameObject consumable;
    public GameObject enemyTank;
    public GameObject enemyBase;
    private GameObject basePositionHolder;
    private float tankWaitTime = 0.0f;
    private GameObject lastKnownEnemyData;
    private GameObject enemyBasePosition;
    public List<GameObject> currentBases;



    public Dictionary<GameObject, float> enemyTanksFound = new Dictionary<GameObject, float>();     // if the enenmy tank is visible it willl be first stored in this dicionary and cna be accessed through the first key
    public Dictionary<GameObject, float> consumablesFound = new Dictionary<GameObject, float>();    // stores any consumables visible 
    public Dictionary<GameObject, float> enemyBasesFound = new Dictionary<GameObject, float>();     // stores any bases visible 
    public HeuristicMode heuristicMode; // change the heuristic method whihc will determine how the tank will pathfind and calclate the distances between the neighbouring nodes(impacting the gcost and hcost for each node therefore changing the path) 

    // enums for prioirity these can be obtained through prioritites.name 
    private void Awake()
    {

        initStateMachine();


    }
    public Vector3 getBasePosition()
    {

        if (currentBases[0] != null) {

            return currentBases[0].transform.position;


        }
        if (currentBases[1] != null)
        {

            return currentBases[1].transform.position;


        }
        return Vector3.zero;
    }
    public bool isBasesALive()
    {
        Debug.Log(currentBases.Count);
        if (currentBases[0] != null)
        {
            return true;
        }
        return false;

    }
    private void initStateMachine()
    {

        // the state machine being passed into certain constructors will get casted to BaseAIBehaviour which gives a
        // transition context to certain states like the retreat state and wait state allowing the
        // states to set and adjust their values based on the previous state only this does not mean every single state is aware of every state or the state machine
        // simply that they have some kind of AI behaviour that has a global context they can access via their previous state or themselves
       

       
      
        if (!TryGetComponent(out CC_SmartTankRBS rules) && !TryGetComponent(out CC_SmartTankBT behaviourTree)){
            Debug.Log("found did not find RBS ");
            Dictionary<Type, BaseST> states = new Dictionary<Type, BaseST>
        {
            {typeof(SearchState),new SearchState(this)},
            {typeof(CC_AttackState),new CC_AttackState(this)},
            {typeof(Retreat),new Retreat(this,GetComponent<CC_FSM>())},
            {typeof(WaitState),new WaitState(GetComponent<CC_FSM>(),this)},
            {typeof(Chase),new Chase(this)},
            {typeof(DodgeState),new DodgeState(this)},
            {typeof(Ambush),new Ambush(this)},
            {typeof(Guard),new Guard(this)},
        };
            GetComponent<CC_FSM>().setStates(states);
        }




    }





    public override void AITankStart()
    {
        Debug.Log("base start");
        // store current bases 
        currentBases = MyBases;
        /// lower thesh holds, higher thresh holds and max for each resource 

        maxHealth = a_GetHealthLevel;
        maxAmmo = a_GetAmmoLevel;
        maxFuel = a_GetFuelLevel;

        lastKnownEnemyData = new GameObject();
        enemyBasePosition = new GameObject();
        // thresh holds used by prirotiy manager to determine which list each priority is placed in(ammo,health,fuel)
        healthPriorityThresh = 30.0f;
        healthSafteyThresh = 50.0f;

        ammoPriorityThresh = 4.0f;
        ammoSafteyThresh = 10.0f;

        fuelPriorityThresh = 40.0f;
        fuelSafteyThresh = 55.0f;
        //wrappers for the values of each resource so they can be passed by reference to the priority holders that will then be sorted by the priority manager 

        healthValuesHolder = new PriorityValuesHolder(healthPriorityThresh, healthSafteyThresh, maxHealth);
        fuelValuesHolder = new PriorityValuesHolder(fuelPriorityThresh, fuelSafteyThresh, maxFuel);
        ammoValuesHolder = new PriorityValuesHolder(ammoPriorityThresh, ammoSafteyThresh, maxAmmo);



        List<PriorityHolder> currentPriorites = new List<PriorityHolder> // create new list of all priorties that need to be managed 
        {
             new PriorityHolder(PRIORITIES.FUEL, PriorityManager.queuePriority.SAFE, fuelValuesHolder) ,
             new PriorityHolder(PRIORITIES.HEALTH, PriorityManager.queuePriority.SAFE, healthValuesHolder) ,
             new PriorityHolder(PRIORITIES.AMMO, PriorityManager.queuePriority.SAFE, ammoValuesHolder) ,
        };
        basePositionHolder = new GameObject();
        basePositionHolder.transform.position = MyBases[0].transform.position;




        // instantiate prriority manager using list of defined prioity holders  
        priorityManager = new PriorityManager(currentPriorites);

        if (wasHit)
        {
            hitTimer -= Time.deltaTime;
            wasHit = hitTimer < 0;
        }




    }
    public override void AIOnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            hitTimer = 1.5f;
            wasHit = true;

        }




    }



    public override void AITankUpdate()
    {
        // checking for any targets/consumables
        enemyBasesFound = a_BasesFound;
        enemyTanksFound = a_TanksFound;
        consumablesFound = a_ConsumablesFound;
        // checking for enemy tanks and bases 
        if (enemyTanksFound.Count > 0 && enemyTanksFound.First().Key != null)
        {
            enemyTank = enemyTanksFound.First().Key;
            lastKnownEnemyData.transform.position = enemyTank.transform.position; ;
            lastKnownEnemyData.transform.forward = enemyTank.transform.forward;
        }
        else
        {
            enemyTank = null;
        }
        if (enemyBasesFound.Count > 0 && enemyBasesFound.First().Key != null)
        {
            enemyBase = enemyBasesFound.First().Key;
            enemyBasePosition.transform.position = enemyBase.transform.position;
        }
        else
        {
            enemyBase = null;
        }
        
        // updating current percent values for resources 
        healthValuesHolder.CurrentPriorityVal = a_GetHealthLevel / maxHealth;
        ammoValuesHolder.CurrentPriorityVal = a_GetAmmoLevel / maxAmmo;
        fuelValuesHolder.CurrentPriorityVal = a_GetFuelLevel / maxFuel;
        currentBases = MyBases;

        priorityManager.Update();// updating the priority queues of the priroity manager based on the percents above 
        wasHit = false;
    }




    public bool stopAndCheckPos(GameObject position, float waitTime, GameObject checkFor, ref float timer)
    {
        

        if (timer < waitTime)
        {
            timer += Time.deltaTime;
            Debug.Log("tank stopping and checking position wait time: " + waitTime);

            if (checkFor != null)
            {
                Debug.Log("wait interupted object found at wait time : " + tankWaitTime);
                timer = 0.0f;
                return true;
            }

            Debug.Log("waiting for " + timer);
            a_FaceTurretToPoint(position);
            return false;
        }
        timer = 0.0f;
        Debug.Log("wait finished tank wait time  " + timer);
        return true;






    }

    public bool compareDistanceBetwenPoints(Vector3 pointToCheck, Vector3 pointToCompareTo)
    {
        return Vector3.Distance(pointToCheck, transform.position) < Vector3.Distance(pointToCheck, pointToCompareTo);

    }
    public float getDistanceToEnemy()
    {

        if (enemyTank != null)
        {
            return Vector3.Distance(lastKnownEnemyData.transform.position, transform.position);

        }

        Debug.Log("tried to get distance to enemy tank but was null returned 0.0f");
        return 0.0f;

    }

    public float getDistanceToEnemyBase()
    {
        if (enemyBase != null)
        {
            return Vector3.Distance(enemyBasePosition.transform.position, transform.position);
        }

        return 0.0f;
    }

    public Vector3 EtankLastKnownTransformForward
    {
        get { return lastKnownEnemyData.transform.forward;  }
    }
    public Vector3 urrentSafteySpot{

        set { currentSafteySpot = value; }
    }
    public float TankFiringDistance
    {
        get { return tankFiringDistance; }
    }
    
    public float BaseFiringDistance
    {
        get { return enemyBaseFiringDistance; }
    }
    public float TankWaitTimer {
        get { return tankWaitTime; }
        set { tankWaitTime = value; }
    

    
    }

    public GameObject LastKnownEPos
    {
        get { return lastKnownEnemyData; }
    }

    public GameObject EnemyBasePos
    {
        get { return enemyBasePosition; }
    }
    public bool WasHit
    {

        get { return wasHit; }

    }
    public Vector3 BasePositionStore
    {
        get{ return basePositionHolder.transform.position; }
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
