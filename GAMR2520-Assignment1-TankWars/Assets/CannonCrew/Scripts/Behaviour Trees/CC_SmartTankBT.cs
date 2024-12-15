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


public class CC_SmartTankBT : CC_SmartTank //SmartTankBT inherits from the regular smart tank to be able to use its methods and functions
{
    //Fatcs and the actions that the tank should do.
    Dictionary<string, bool> facts = new Dictionary<string, bool>();
    List<string> shouldChase = new List<string>();
    List<string> shouldAttack = new List<string>();
    List<string> shouldRetreat = new List<string>();
    List<string> shouldGetPickup = new List<string>();

    CC_BTSearch searchState;
    CC_BTChase chaseState;
    CC_BTAttack attackState;
    CC_BTNewRetreat retreatState;

    float retreatTimer = 0.0f;
    float retreatTimerMax = 10.5f;

    //Behavior tree
    void RunBT()
    {
        //We should only chase if we see an enemy and our health is high or if we see an enemy base 
        if (Sequence(shouldChase) || facts["seeEnemyBases"])
        {
            chaseState.Update();
        }

        //We attack if we are high health, within enemy range and have ammo or if we are within the range of the enemy base.
        //We dont need to do extra checks for attacking bases because it's a stationary target so as long as we have ammo we can destroy it.
        if (Sequence(shouldAttack) || facts["withinBaseRange"])
        {
            attackState.Update();
        }

        //We should only retreat if we are low health.
        //The bReturn is so that after reatreating and stopping for a certain amount of time we go back into the search state.
        //It essentially acts as a check.
        if (Sequence(shouldRetreat) && retreatState.bReturn == false)
        {
            //Whilst in the retreat state we check if we see a pickup and if we do we go and collect it.
            //This means we can go collect a valuable resource so we can break out of the retreat stae.
            if (Sequence(shouldGetPickup))
            {
                PickUps();
            }

            //Otherwise we just keep running away.
            else
            {
                retreatState.Update();
            }
            
        }

        //Of course we need to be able to get pickups when we are not in the retreat so this does that.
        else if(Sequence(shouldGetPickup))
        {
            PickUps();
        }

        //Search is the default we go into if none of the other actions are done.
        //This is to make sure the tank isn't constantly changing into search after one of the other actions has been hit.
        //Because that causes the A* path to change constantly and make the tank move wrong.
        if (!(Sequence(shouldChase) || facts["seeEnemyBases"]) && !(Sequence(shouldAttack) && facts["withinBaseRange"]) 
            && !(Sequence(shouldRetreat) && retreatState.bReturn == false) && !Sequence(shouldGetPickup))
        {
            searchState.Update();

            //Once we are back into search we need to set bReturn to false so that we can go back into retreat again.
            if (this.enemyTank != null)
            {
                retreatState.bReturn = false;
            }
        }

    }




    void InitialiseFacts()
    {
        //Initialise all of our facts 
        //Not all of these have been used but were there if we needed to expand upon the behavior tree.
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

        //Getting Pickups
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
        //All of our facts checked every frame
        facts["cantSeeEnemy"] = enemyTank == null ? true : false;
        facts["cantSeeBases"] = enemyBase == null ? true : false;

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

        facts["seePickup"] = this.consumablesFound.Count > 0 ? true : false;
    }

    //This function makes use of the functions in the search state that are related to collecting pickups.
    //We have decided to call them separately from the search state in order to stop the tank from constantly generating new paths.
    //When it see's a pickup and just focuses on that one pickup alone.
    void PickUps()
    {
        searchState.organiseConsumables();
        searchState.EvaluatePriorityPositions();
        searchState.MoveToPriorityPositions();
    }

 
    public override void AITankStart()
    {
        base.AITankStart();


        InitialiseFacts();
        InitialiseLists();
        searchState = new CC_BTSearch(this);
        chaseState = new CC_BTChase(this);
        attackState = new CC_BTAttack(this);
        retreatState = new CC_BTNewRetreat(this);
   
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
