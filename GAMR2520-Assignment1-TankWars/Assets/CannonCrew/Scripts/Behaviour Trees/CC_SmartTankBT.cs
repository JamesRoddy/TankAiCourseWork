using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;
using static AStar;
using static CC_SmartTank;
using static PriorityManager;




public class CC_SmartTankBT : CC_SmartTank
{

    Dictionary<string, bool> facts = new Dictionary<string, bool>();
    List<string> shouldSearch = new List<string>();
    List<string> shouldChase = new List<string>();
    List<string> shouldAttack = new List<string>();
    List<string> shouldRetreat = new List<string>();
    List<string> shouldVisionLostChase = new List<string>();

    CC_BTSearch searchState;
    CC_BTChase chaseState;
    CC_BTAttack attackState;
    CC_BTRetreat retreatState;
    CC_BTWait wait;


    public BTActionNode fuelCheck;
    public BTActionNode healthCheck;
    public BTActionNode ammoCheck;
    public BTSequence search;
    float retreatTimer = 0.0f;
    float retreatTimerMax = 10.5f;
    float chaseTime = 10.0f;
    float t = 0.0f;

    float tankChecKBehindeTime = 0.5f;
    bool wasAttack = false;

    //private void Awake()
    //{

    //initStateMachine();
    //InitialiseBT();

    //}

    private void Awake()
    {
        
    }


    private void initStateMachine()
    {


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


            Debug.Log("found did not find RBS ");
            GetComponent<CC_FSM>().setStates(states);
        




    }

    /*private void InitialiseBT()
    {
        fuelCheck = new BTActionNode(FuelCheck);
        healthCheck = new BTActionNode(HealthCheck);
        ammoCheck = new BTActionNode(AmmoCheck);
        search = new BTSequence(new List<BTBaseNode> { fuelCheck, healthCheck });
    }*/

    void RunBT()
    {

        /*Debug.LogError("Sequence should chase: " + Sequence(shouldChase));
        //Debug.LogError("Sequence should lost vision chase: " + Sequence(shouldVisionLostChase));
        if (Selection(shouldChase))
        {
            chaseState.Update();
        }

        else if(Sequence(shouldVisionLostChase))
        {
            chaseState.Update();
           Debug.LogError("Lost vision chase");
        }

        if (Sequence(shouldAttack) || facts["seeEnemyBases"])
        {
            Debug.Log("Trying to attack");
            attackState.Update();
            wasAttack = true;
        }

        if (Sequence(shouldRetreat))
        {
            Debug.Log("Retreat");
            retreatTimer += Time.deltaTime;
            retreatState.Update();

        }

        else
        {
            searchState.Update();
        }*/

        if (Sequence(shouldChase))
        {
            //Debug.Log("Entered chase");

            chaseState.Update();

            //Debug.Log("Exited chase");
        }

        else if (Sequence(shouldVisionLostChase))
        {
            chaseState.Update();
            Debug.LogError("Lost vision chase");
        }

        if (Sequence(shouldAttack) || facts["seeEnemyBases"])
        {
            Debug.Log("Trying to attack");
            attackState.Update();
        }

        if (Sequence(shouldRetreat))
        {
            Debug.Log("Retreat");
            retreatTimer += Time.deltaTime;
            retreatState.Update();

        }

        if(!Sequence(shouldChase) && !Sequence(shouldVisionLostChase) && !(Sequence(shouldAttack) || facts["seeEnemyBases"]) && !Sequence(shouldRetreat))
        {
            searchState.Update();
        }


    }




    void InitialiseFacts()
    {
        facts.Add("lowHealth", false); // our health is low                                       
        facts.Add("lowFuel", false); // our fuel is low                                           
        facts.Add("lowAmmo", false); // our ammo is low                                           
        facts.Add("highHealth", false); // our health is high                                     
        facts.Add("highFuel", false); // our fuel is high                                         
        facts.Add("highAmmo", false); // our ammo is high                                         
        facts.Add("enemySeen", false); // we see the enemy tank
        facts.Add("cantSeeEnemy", false);
        facts.Add("withinTankRange", false); // we are within the firing range of the enemy
        facts.Add("withinBaseRange", false);
        facts.Add("canAttack", false); // a fact to combine other facts like enemy seen, high fuel and high ammo into 1
        facts.Add("shouldRetreat", false); // combining the enemySeen and lowHealth facts into one fact
        facts.Add("shouldChase", false); // we should retreat because we see the enemy, our health is low and we aren't retreating already
        facts.Add("seePickUp", false);
        facts.Add("cantSeePickup", false);
        facts.Add("seeEnemyBases", false);
        facts.Add("cantSeeBases", false);
        facts.Add("atBases", false);
        facts.Add("isInRetreat", false);
        facts.Add("retreatTimer", false);
        facts.Add("chaseTimer", false);
    }

    void InitialiseLists()
    {
        //Searching Facts
        //shouldSearch.Add("cantSeeEnemy");
        //shouldSearch.Add("cantSeePickup");
        //shouldSearch.Add("cantSeeBases");

        //Chase
        shouldChase.Add("highHealth");
        shouldChase.Add("enemySeen");
        //shouldChase.Add("seeEnemyBases");
        //shouldChase.Add("cantSeeEnemy");

        //Attack
        //shouldAttack.Add("enemySeen");
        //shouldAttack.Add("seeEnemyBases");
        shouldAttack.Add("withinTankRange");
        shouldAttack.Add("highHealth");
        
        //shouldAttack.Add("withinBaseRange");

        //Retreat
        shouldRetreat.Add("lowHealth");
        //shouldRetreat.Add("notAtBases");
        shouldRetreat.Add("retreatTimer");

        //shouldRetreat.Add("enemySeen");

        shouldVisionLostChase.Add("chaseTimer");
        shouldVisionLostChase.Add("cantSeeEnemy");
        shouldVisionLostChase.Add("cantSeeBases");
    }

    bool Selection(List<string> conditions)
    {
        foreach (var item in conditions)
        {
            if (facts[item] == true)
            {
                return true;
            }

            else
            {
                continue;
            }

        }

        return false;
    }

    bool Sequence(List<string> conditions)
    {
        foreach (var item in conditions)
        {
            if (facts[item])
            {
                continue;
            }

            else
            {
                return false;
            }

        }
        return true;
    }

    void UpdateFacts()
    {
        facts["cantSeeEnemy"] = enemyTank == null ? true : false;
        facts["cantSeeBases"] = enemyBase == null ? true : false;

        Debug.Log("Facts[cantSeeEnemy]: " + facts["cantSeeEnemy"]);
        Debug.Log("Facts[cantSeeBases]: " + facts["cantSeeBases"]);

        facts["highHealth"] = priorityManager.checkHigh(PRIORITIES.HEALTH) ? true : false;
        facts["enemySeen"] = enemyTank != null ? true : false;
        facts["seeEnemyBases"] = enemyBase != null ? true : false;

        facts["withinTankRange"] = enemyTank != null && (Vector3.Distance(transform.position, enemyTank.transform.position) < 30f &&
           Vector3.Distance(transform.position, enemyTank.transform.position) > 5f) ?  true : false; 
        facts["withinBaseRange"] = enemyBase != null && Vector3.Distance(transform.position, EnemyBasePos.transform.position) < 10f ? true : false;

        facts["lowHealth"] = priorityManager.checkLow(PRIORITIES.HEALTH) ? true : false;
        facts["notAtBases"] = Vector3.Distance(transform.position,BasePositionStore) > 30f ? true : false;

       
        facts["isInRetreat"] = Sequence(shouldRetreat) ? true : false;
        facts["retreatTimer"] = retreatTimer <= retreatTimerMax ? true : false;
        //Debug.Log("retreatTimer: " + retreatTimer);
        //Debug.Log("Facts[retreatTimer]: " + facts["retreatTimer"]);

        if (!facts["retreatTimer"])
        {
            //Debug.Log("Going to wait");
            wait.Update();
            //retreatTimer = 0;
            
        }

        facts["chaseTimer"] = t > chaseTime ? true : false;
        if (!facts["chaseTimer"])
        {
            t+= Time.deltaTime; 
        }

        else
        {
            t = 0;
        }

    }

    /* public BTNodeState FuelCheck()
     {
         if(priorityManager.checkLow(PRIORITIES.FUEL))
         {
             return BTNodeState.FAILURE;
         }
         else
         {
             Searching();
             return BTNodeState.SUCCESS;
         }
     }

     public BTNodeState HealthCheck()
     {
         if (priorityManager.checkLow(PRIORITIES.HEALTH))
         {
             return BTNodeState.FAILURE;
         }
         else
         {
             return BTNodeState.SUCCESS;
         }
     }

     public BTNodeState AmmoCheck()
     {
         if (priorityManager.checkLow(PRIORITIES.AMMO))
         {
             return BTNodeState.FAILURE;
         }
         else
         {
             return BTNodeState.SUCCESS;
         }
     }*/


    public override void AITankStart()
    {
        Debug.Log("starting BT");
        base.AITankStart();


        InitialiseFacts();
        InitialiseLists();
        searchState = new CC_BTSearch(this);
        chaseState = new CC_BTChase(this);
        attackState = new CC_BTAttack(this);
        retreatState = new CC_BTRetreat(this, wait);
        wait = new CC_BTWait(this);
   
    }
    public override void AIOnCollisionEnter(Collision collision)
    {

        base.AIOnCollisionEnter(collision);

    }



    public override void AITankUpdate()
    {
        
        base.AITankUpdate();
        UpdateFacts();
        RunBT();
    }



   

}
