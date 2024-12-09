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
    // taking into account the extra health ammo and fuel that can be collected(when caclulating the maxiumum for each resource)
    private float ammoMaxOffset = 5.0f;
    private float fuelMaxOffset = 25.0f;
    private float healthMaxOffset = 25.0f;
    private float tankFiringDistance = 40.0f;
    private float enemyBaseFiringDistance = 30.0f;
    
    private void Awake()
    {

        initStateMachine();


    }
    private void initStateMachine()
    {


        Dictionary<Type, BaseST> states = new Dictionary<Type, BaseST>
        {
            {typeof(SearchState),new SearchState(this)},
            {typeof(CC_AttackState),new CC_AttackState(this)},
            {typeof(Retreat),new Retreat(this)},
            {typeof(Chase),new Chase(this)},
            {typeof(DodgeState),new DodgeState(this)},
            {typeof(Ambush),new Ambush(this)},
            {typeof(Guard),new Guard(this)},
        };

        if (!TryGetComponent(out CC_SmartTankRBS rules)) {
            Debug.Log("found did not find RBS ");
            GetComponent<CC_FSM>().setStates(states);
        }




    }

    private void InitialiseBt()
    {

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
