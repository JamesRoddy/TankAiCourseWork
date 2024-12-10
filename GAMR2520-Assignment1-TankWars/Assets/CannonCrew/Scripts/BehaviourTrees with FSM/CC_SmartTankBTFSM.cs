using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static AStar;
using static CC_SmartTank;
using static PriorityManager;




public class CC_SmartTankBTFSM : CC_SmartTank
{

    Dictionary<string, bool> facts = new Dictionary<string, bool>();
    List<string> shouldSearch = new List<string>();
    List<string> shouldChase = new List<string>();
    List<string> shouldAttack = new List<string>();
    List<string> shouldRetreat = new List<string>();

    public BTActionNode fuelCheck;
    public BTActionNode healthCheck;
    public BTActionNode ammoCheck;
    public BTSequence search;


    private void Awake()
    {

        initStateMachine();
        //InitialiseBT();

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
            {typeof(Ambush),new Ambush(this,GetComponent<CC_FSM>())},
            {typeof(Guard),new Guard(this)},
        };


            Debug.Log("found did not find RBS ");
            GetComponent<CC_FSM>().setStates(states);
        




    }

    private void InitialiseBT()
    {
        fuelCheck = new BTActionNode(FuelCheck);
        healthCheck = new BTActionNode(HealthCheck);
        ammoCheck = new BTActionNode(AmmoCheck);
        //search = new BTSequence(new List<BTBaseNode> { fuelCheck, healthCheck });
    }

    /*void Initialisefacts()
    {
        facts.Add("lowHealth", false); // our health is low                                       
        facts.Add("lowFuel", false); // our fuel is low                                           
        facts.Add("lowAmmo", false); // our ammo is low                                           
        facts.Add("highHealth", false); // our health is high                                     
        facts.Add("highFuel", false); // our fuel is high                                         
        facts.Add("highAmmo", false); // our ammo is high                                         
        facts.Add("enemySeen", false); // we see the enemy tank                                                      
        facts.Add("withinRange", false); // we are within the firing range of the enemy
        facts.Add("canAttack", false); // a fact to combine other facts like enemy seen, high fuel and high ammo into 1
        facts.Add("shouldRetreat", false); // combining the enemySeen and lowHealth facts into one fact
        facts.Add("shouldChase", false); // we should retreat because we see the enemy, our health is low and we aren't retreating already
    }*/

    public BTNodeState FuelCheck()
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
