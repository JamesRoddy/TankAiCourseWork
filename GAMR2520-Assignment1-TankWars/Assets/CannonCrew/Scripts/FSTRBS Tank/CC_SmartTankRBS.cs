using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AStar;
using static CC_SmartTank;
using static PriorityManager;




public class CC_SmartTankRBS : CC_SmartTank
{
    public Dictionary<string, bool> stats = new Dictionary<string, bool>();
    public Rules rules = new Rules();


    private void Awake()
    {
        InitiliseStats();
        InitiliseRules();
        initStateMachine();
        //wrappers for the values of each resource so they can be passed by reference to the priority holders that will then be sorted by the priority manager 
        // current thresholds for when something should become a priority
        // calc maxiumum for resources 

    }

    public void InitiliseStats()
    {
        stats.Add("attackState", false); //we are in the attack state
        stats.Add("searchState", false); //we are in the search state
        stats.Add("retreatState", false); //we are in the retreat state
        stats.Add("chaseState", false); //we are in the chase state                               
        stats.Add("lowHealth", false); // our health is low                                       
        stats.Add("lowFuel", false); // our fuel is low                                           
        stats.Add("lowAmmo", false); // our ammo is low                                           
        stats.Add("highHealth", false); // our health is high                                     
        stats.Add("highFuel", false); // our fuel is high                                         
        stats.Add("highAmmo", false); // our ammo is high                                         
        stats.Add("enemySeen", false); // we see the enemy tank                                   
      //  stats.Add("ammoNotMajor", false); // our ammo is not a major priority                   
        stats.Add("withinRange", false); // we are within the firing range of the enemy           
    }

    public void InitiliseRules()
    {
        rules.addRule(new Rule("enemySeen", "lowHealth", typeof(RetreatRBS), Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat
    }
    public void CheckHealth()
    {
        if (priorityManager.checkLow(PRIORITIES.HEALTH) == true) // checks if we are low on health
        {
            stats["lowHealth"] = true; // returns the lowHealth fact as true
        }
        else if (priorityManager.checkHigh(PRIORITIES.HEALTH) == true) // if we aren't low on health, then check if we are high on health
        {
            stats["highHealth"] = true; // if we are then returns highHealth as true
        }

        else // otherwise set both to false
        {
            stats["lowHealth"] = false;
            stats["highHealth"] = false;
        }
    }

    public void CheckFuel()
    {
        if (priorityManager.checkLow(PRIORITIES.FUEL)  == true) // checks if we are low on fuel
        {
            stats["lowFuel"] = true; // if we are then return true
        }
        else if (priorityManager.checkHigh(PRIORITIES.FUEL)) // if not then check if we are high on fuel
        {
            stats["highFuel"] = true; // if yes then return as true
        }
        else // otherwise both return as false
        {
            stats["lowFuel"] = false;
            stats["highFuel"] = false;
        }
    }

    public void checkAmmo()
    {
        if (priorityManager.checkLow(PRIORITIES.AMMO) == true)
        {
            stats["lowAmmo"] = true;
        }
        else if (priorityManager.checkHigh(PRIORITIES.AMMO) == true)
        {
            stats["highAmmo"] = true;
        }
        else
        {
            stats["lowAmmo"] = false;
            stats["highAmmo"] = false;
        }
    }

    public void SetEnemySeen()
    {
        if (enemyTank != null)
        {
            stats["enemySeen"] = true;
        }
        else
        {
            stats["enemySeen"] = false;
        }
    }

    public void IsWithinRange()
    {
        if(getDistanceToEnemy() < TankFiringDistance)
        {
            stats["withinRange"] = true;
        }
        else
        {
            stats["withinRange"] = false;
        }
    }
    private void initStateMachine()
    {

        Dictionary<Type, BaseST> states = new Dictionary<Type, BaseST>
        {
           {typeof(SearchStateRBS),new SearchStateRBS(this)},
            {typeof(CC_AttackStateRBS),new CC_AttackStateRBS(this)},
            {typeof(RetreatRBS),new RetreatRBS(this)},
            {typeof(ChaseRBS),new ChaseRBS(this)}
        };


        GetComponent<CC_FSM>().setStates(states);


    }





    public override void AITankStart()
    {
        base.AITankStart();



        





    }
    public override void AIOnCollisionEnter(Collision collision)
    {
        base.AIOnCollisionEnter(collision);


    }



    public override void AITankUpdate()
    {
        base.AITankUpdate();


    }
}