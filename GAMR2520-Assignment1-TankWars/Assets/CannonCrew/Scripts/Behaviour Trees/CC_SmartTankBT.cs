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
    List<string> shouldGetPickup = new List<string>();

    CC_BTSearch searchState;
    CC_BTChase chaseState;
    CC_BTAttack attackState;
    CC_BTNewRetreat retreatState;
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
    //InitialiseBT()
    //}

    private void Awake()
    {
        
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
       

        if (Sequence(shouldChase) || facts["seeEnemyBases"])
        {
            chaseState.Update();
        }

        /*else if (Sequence(shouldVisionLostChase))
        {
            chaseState.LostVisionChase();
            Debug.LogError("Lost vision chase");
        }*/

        if (Sequence(shouldAttack) || facts["withinBaseRange"])
        {
            Debug.Log("Trying to attack");
            attackState.Update();
        }

        if (Sequence(shouldRetreat) && retreatState.bReturn == false)
        {
            Debug.Log("Retreat");
            retreatState.Update();
            Debug.Log("Exit retreat");
        }

        else if(Sequence(shouldGetPickup))
        {
            Debug.Log("shouldGetPickup");
            searchState.organiseConsumables();
            searchState.EvaluatePriorityPositions();
            searchState.MoveToPriorityPositions();
        }

        if (!(Sequence(shouldChase) || facts["seeEnemyBases"]) && 
            !Sequence(shouldVisionLostChase) && !(Sequence(shouldAttack) && facts["withinBaseRange"]) 
            && !(Sequence(shouldRetreat) && retreatState.bReturn == false) && !Sequence(shouldGetPickup))
        {
            searchState.Update();

            if (this.enemyTank != null)
            {
                retreatState.bReturn = false;
            }
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
        facts.Add("seePickup", false);
    }

    void InitialiseLists()
    {

        //Chase
        shouldChase.Add("highHealth");
        shouldChase.Add("enemySeen");

        //Attack
        shouldAttack.Add("withinTankRange");
        shouldAttack.Add("highHealth");
        shouldAttack.Add("highAmmo");
        
        //Retreat
        shouldRetreat.Add("lowHealth");

        shouldVisionLostChase.Add("chaseTimer");
        shouldVisionLostChase.Add("cantSeeEnemy");
        shouldVisionLostChase.Add("cantSeeBases");

        shouldGetPickup.Add("seePickup");
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

        //Debug.Log("Facts[cantSeeEnemy]: " + facts["cantSeeEnemy"]);
        //Debug.Log("Facts[cantSeeBases]: " + facts["cantSeeBases"]);

        facts["highHealth"] = priorityManager.checkHigh(PRIORITIES.HEALTH) ? true : false;
        facts["enemySeen"] = enemyTank != null ? true : false;
        facts["seeEnemyBases"] = enemyBase != null ? true : false;

        facts["withinTankRange"] = enemyTank != null && (Vector3.Distance(transform.position, enemyTank.transform.position) < 60f &&
           Vector3.Distance(transform.position, enemyTank.transform.position) > 10f) ?  true : false; 
        facts["withinBaseRange"] = enemyBase != null && (Vector3.Distance(transform.position, EnemyBasePos.transform.position) < 30f && 
            Vector3.Distance(transform.position, EnemyBasePos.transform.position) > 10f) ? true : false;
        facts["lowHealth"] = priorityManager.checkLow(PRIORITIES.HEALTH) ? true : false;

        facts["highAmmo"] = !priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO) ? true : false;

        facts["isInRetreat"] = Sequence(shouldRetreat) ? true : false;
        facts["retreatTimer"] = retreatTimer <= retreatTimerMax ? true : false;
        //Debug.Log("retreatTimer: " + retreatTimer);
        //Debug.Log("Facts[retreatTimer]: " + facts["retreatTimer"]);

        /*if (!facts["retreatTimer"])
        {
            //Debug.Log("Going to wait");
            wait.Update();
            //retreatTimer = 0;
            
        }*/

        facts["chaseTimer"] = t > chaseTime ? true : false;
        if (!facts["chaseTimer"])
        {
            t+= Time.deltaTime; 
        }

        else
        {
            t = 0;
        }

       
        facts["seePickup"] = this.consumablesFound.Count > 0 ? true : false;
        Debug.Log("seePickup: " + facts["seePickup"]);

    }

    /*public BTNodeState FuelCheck()
     {
         if(priorityManager.checkLow(PRIORITIES.FUEL))
         {
             return BTNodeState.FAILURE;
         }
         else
         {
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
        retreatState = new CC_BTNewRetreat(this);
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
