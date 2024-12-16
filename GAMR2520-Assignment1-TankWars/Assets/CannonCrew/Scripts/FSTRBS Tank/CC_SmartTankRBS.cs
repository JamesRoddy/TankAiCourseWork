using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneTemplate;
using UnityEngine;
using static AStar;
using static CC_SmartTank;
using static PriorityManager;




public class CC_SmartTankRBS : CC_SmartTank
{
    public Dictionary<string, bool> stats = new Dictionary<string, bool>();
    public Rules rules = new Rules();
    private float chaseTime = 0.0f;
    private float chaseTimeMax = 4.0f;
    private float chaseDistanceCheck = 5.0f;
    CC_ChaseStateRBS chaseDebug;
    CC_SmartTankRBS debugTank;
    CC_AttackStateRBS attackDebug;
    CC_RetreatStateRBS retreatDebug;
    Coroutine firiring;
    CC_SearchStateRBS searchDebug;


    public Dictionary<string, Dictionary<string, bool>> multiStatQueryForBT; // allows us to get stats for the behaviour tree side of the FSMRBSBT ommmiting certain things like if a state is false or true as that does not need to be included in a stats query from the bt behaviours  
    // mini rule dictionaries allow us to check multiple booleans at once and predefine all the conditions that need to be met 
    // used to check if we should attack
    Dictionary<string, bool> canAttackCheck = new Dictionary<string, bool>
    {
        {"enemySeen",true },
        {"highHealth",true},
        { "ammoCritical",false},
        { "shouldRetreat",false},
        { "attackState",false}
    };

    Dictionary<string, bool> canAttackEnemyBaseCheck = new Dictionary<string, bool>
    {
        {"enemyBaseSeen",true },
        {"highHealth",true},
        { "ammoCritical",false},
        { "shouldRetreat",false},
        { "attackState",false}
    };



    Dictionary<string, bool> shouldChaseBaseCheck = new Dictionary<string, bool>
    {

        {"enemyBaseWithinRange",false },
        { "enemyBaseSeen",true },
        { "ammoCritical",false },
        { "chaseState",false},

    };
    // used to check if we should chase 
    Dictionary<string, bool> shouldChaseCheck = new Dictionary<string, bool>
    {

        {"withinRange",false },
        { "enemySeen",true },
        { "highFuel",true },
        { "highHealth", true },
        { "ammoCritical",false },
        { "chaseState",false},

    };


    Dictionary<string, bool> shouldChaseAttackCheck = new Dictionary<string, bool>
    {

        {"withinRange",false },
        { "enemySeen",true },
        { "highFuel",true },
        { "highHealth", true },
        { "ammoCritical",false },
        { "chaseState",false},

    };
    // used to check if enemy dropped out of sight during attack
    Dictionary<string, bool> shouldChaseTargetNotVisible = new Dictionary<string, bool>
    {
        {"wasWithinRange",true },
        {"chaseTimer",false },
        {"enemyBaseSeen",false },
        { "enemySeen",false },
        { "highFuel",true },
        { "ammoCritical",false },
        { "highHealth", true },



    };
    // used to deteetct of the enemy dropped out of sight during chase
    Dictionary<string, bool> shouldChaseTargetFromChase = new Dictionary<string, bool>
    {
        {"hadChase",true },
        {"enemyBaseSeen",false },
        {"chaseTimer",false },
        { "enemySeen",false },
        { "highFuel",true },
        { "ammoCritical",false },
        { "highHealth", true },


    };
    // check if chasing abase to gte in range is viable
    Dictionary<string, bool> shouldChaseBase = new Dictionary<string, bool>
    {


        {"enemyBaseWithinRange",false },
        { "enemySeen",false },
        {"enemyBaseSeen",true },
        { "highFuel",true },
        { "ammoCritical",false },
        { "highHealth", true },
        {"chaseState",false }

    };


    // used to detect if we should retreat our not 
    Dictionary<string, bool> shouldRetreatCheck = new Dictionary<string, bool>
    {
        { "lowHealth",true },
        { "enemySeen",true },
        { "retreatState",false }
    };

    List<Dictionary<string, bool>> backwardsChainFromSearchToChaseConditions;
    List<Rule> rulesForRbs;

    private void Awake()
    {
        debugTank = new CC_SmartTankRBS();
        chaseDebug = new CC_ChaseStateRBS(debugTank);
        attackDebug = new CC_AttackStateRBS(debugTank);
        retreatDebug = new CC_RetreatStateRBS(debugTank);
        searchDebug = new CC_SearchStateRBS(debugTank);
        Debug.Log("awake called for rbs tank");
        rulesForRbs = new List<Rule>
        {
         new Rule("shouldRetreat", "attackState", typeof(CC_RetreatStateRBS),retreatDebug, Rule.Predicate.And), // if we see the enemy and are on low health then we should retreat
        new Rule("shouldRetreat", "searchState", typeof(CC_RetreatStateRBS), retreatDebug,Rule.Predicate.And), // if we see the enemy and are on low health then we should retreat
        new Rule("shouldRetreat", "chaseState", typeof(CC_RetreatStateRBS),retreatDebug, Rule.Predicate.And), // if we see the enemy and are on low health then we should retreat
        new Rule("withinRange", "canAttack", typeof(CC_AttackStateRBS),attackDebug, Rule.Predicate.And),// if we are able to attack(our health and fuel are high and ammo isn't a major priority) we should go into the attack state
        new Rule("enemyBaseWithinRange", "canAttackBase", typeof(CC_AttackStateRBS), attackDebug, Rule.Predicate.And),
        new Rule("shouldChase", "moveToTarget", typeof(CC_ChaseStateRBS),chaseDebug ,Rule.Predicate.Or), // if we are in the attack state
        new Rule("enemyBaseSeen", "chaseBase", typeof(CC_ChaseStateRBS), chaseDebug, Rule.Predicate.And),

        };
        InitiliseStats();
        InitiliseRules(rulesForRbs);
        initStateMachine();
        initRuleDictionaries();


    }



    public void initRuleDictionaries()
    {
        backwardsChainFromSearchToChaseConditions = new List<Dictionary<string, bool>>()
     {
         shouldChaseTargetNotVisible,
         shouldChaseTargetFromChase,

     };

    }
    public void InitiliseStats()
    {

        Debug.Log("init stats ");
        stats.Add("attackState", false); //we are in the attack state
        stats.Add("searchState", true); //we are in the search state
        stats.Add("retreatState", false); //we are in the retreat state
        stats.Add("chaseState", false); //we are in the chase state                               
        stats.Add("lowHealth", false); // our health is low                                       
        stats.Add("lowFuel", false); // our fuel is low                                           
        stats.Add("lowAmmo", false); // our ammo is low                                           
        stats.Add("highHealth", false); // our health is high                                     
        stats.Add("highFuel", false); // our fuel is high                                         
        stats.Add("highAmmo", false); // our ammo is high                                         
        stats.Add("enemySeen", false); // we see the enemy tank                                                      
        stats.Add("withinRange", false); // we are within the firing range of the enemy
        stats.Add("canAttack", false); // a fact to combine other facts like enemy seen, high fuel and high ammo into 1
        stats.Add("shouldRetreat", false); // combining the enemySeen and lowHealth facts into one fact
        stats.Add("shouldChase", false); // we should retreat because we see the enemy, our health is low and we aren't retreating already
        stats.Add("ammoCritical", false); // our ammo is critical
        stats.Add("enemyBaseSeen", false); // we see the enemy base
        stats.Add("ammoMajor", false);// ammo prioirty is major
        stats.Add("fuelMajor", false);
        stats.Add("healthMajor", false);
        stats.Add("enemyBaseWithinRange", false);// base in range
        stats.Add("chaseEnemy", false);
        stats.Add("chaseTimer", false);
        stats.Add("search", false);
        stats.Add("moveToTarget", false);
        stats.Add("lostSight", false);
        stats.Add("wasWithinRange", false);
        stats.Add("hadChase", false);
        stats.Add("canAttackBase", false);
        stats.Add("chaseBase", false);

        if (TryGetComponent(typeof(CC_smartTankFSMRBSBT), out var tankFSMRBSBT))
        {
            Debug.Log("FSMRBSBTfound initing multi stat query");
            multiStatQueryForBT = new Dictionary<string, Dictionary<string, bool>>
            {
            {"canAttack",canAttackCheck },
            {"chaseBase",shouldChaseBase },
            {"canAttackBase",canAttackEnemyBaseCheck },
            {"shouldChase",shouldChaseCheck },
            {"shouldRetreat",shouldRetreatCheck }
            };
        }

    }

    public void InitiliseRules(List<Rule> rulesList)
    {
        foreach (Rule rule in rulesList)
        {

            rules.addRule(rule);
        }
    }


    // check the current health level based on its priority 
    private void CheckHealth()
    {
        if (priorityManager.checkLow(PRIORITIES.HEALTH) == true) // checks if we are low on health
        {

            stats["lowHealth"] = true; // returns the lowHealth fact as true
            stats["highHealth"] = false;

            if (priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.HEALTH))
            {
                stats["majorHealth"] = true;
            }
            else
            {
                stats["majorHealth"] = false;
            }
        }
        else if (priorityManager.checkHigh(PRIORITIES.HEALTH) == true) // if we aren't low on health, then check if we are high on health
        {
            stats["highHealth"] = true; // if we are then returns highHealth as true
            stats["lowHealth"] = false;
            stats["majorHealth"] = false;
        }

        else // otherwise set both to false
        {
            stats["lowHealth"] = false;
            stats["highHealth"] = false;
            stats["healthMajor"] = false;
        }
    }

    //check the current fuel level based on the priority manager
    private void CheckFuel()
    {
        if (priorityManager.checkLow(PRIORITIES.FUEL) == true) // checks if we are low on fuel
        {
            stats["lowFuel"] = true; // if we are then return true
            stats["highFuel"] = false;
            if (priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.FUEL))
            {
                stats["fuelMajor"] = true;
            }
        }
        else if (priorityManager.checkHigh(PRIORITIES.FUEL)) // if not then check if we are high on fuel
        {
            stats["highFuel"] = true;
            stats["lowFuel"] = false;
            stats["fuelMajor"] = false;
        }
        else // otherwise both return as false
        {
            stats["lowFuel"] = false;
            stats["highFuel"] = false;
            stats["fuelMajor"] = false;
        }
    }

    private void checkAmmo()
    {


        if (priorityManager.checkLow(PRIORITIES.AMMO) == true)
        {

            stats["lowAmmo"] = true;
            stats["highAmmo"] = false;

            if (priorityManager.checkQueue(queuePriority.MAJOR, PRIORITIES.AMMO))
            {

                stats["ammoMajor"] = true;
                stats["ammoCritical"] = false;
            }

            if (priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
            {
                stats["ammoCritical"] = true;
                stats["ammoMajor"] = false;
            }
        }
        if (priorityManager.checkHigh(PRIORITIES.AMMO) == true)
        {
            stats["highAmmo"] = true;
            stats["lowAmmo"] = false;
            stats["ammoMajor"] = false;
            stats["ammoCritical"] = false;
        }
        else
        {
            stats["lowAmmo"] = false;
            stats["highAmmo"] = false;
            stats["ammoCritical"] = false;
            stats["ammoMajor"] = false;
        }
    }

    private void SetEnemySeen()
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

    private void SetEnemyBaseSeen()
    {
        if (enemyBase != null)
        {
            stats["enemyBaseSeen"] = true;
        }
        else
        {
            stats["enemyBaseSeen"] = false;
        }
    }

    private void IsWithinRange()
    {
        if (stats["enemySeen"])
        {
            if (Vector3.Distance(enemyTank.transform.position, transform.position) < TankFiringDistance)
            {
                stats["wasWithinRange"] = true;
                stats["withinRange"] = true;
            }
            else
            {
                stats["withinRange"] = false;
            }
            return;
        }
        else
        {


            stats["withinRange"] = false;
        }


    }


    private void CheckCanAttack()
    {
        if (checkSum(canAttackCheck) == true)
        {
            stats["canAttack"] = true;

        }
        else
        {
            stats["canAttack"] = false;

        }
    }

    private void CheckCanAttackEnemyBase()
    {

        if (checkSum(canAttackEnemyBaseCheck) == true)
        {
            stats["canAttackBase"] = true;

        }
        else
        {
            stats["canAttackBase"] = false;


        }

    }

    public void CheckChaseBase()
    {
        if (checkSum(shouldChaseBase))
        {
            stats["chaseBase"] = true;
        }
        else
        {
            stats["chaseBase"] = false;
        }
    }
    private void CheckShouldRetreat()
    {

        if (checkSum(shouldRetreatCheck) == true)
        {

            stats["shouldRetreat"] = true;
        }
        else
        {
            stats["shouldRetreat"] = false;
        }
    }
    private void CheckShouldChase()
    {
        if (stats["chaseState"] == false)
        {
            if (checkSum(shouldChaseCheck))
            {

                stats["hadChase"] = true;
                stats["shouldChase"] = true;
                return;
            }


        }

        stats["shouldChase"] = false;




    }




    public void resetTimersIntoSearch()
    {

        Debug.Log("switch to search");


    }




    private void enemyBaseWithinRange()
    {
        if (stats["enemyBaseSeen"] == true)
        {
            if (Vector3.Distance(transform.position, enemyBase.transform.position) <= BaseFiringDistance)
            {
                stats["enemyBaseWithinRange"] = true;
            }
            else
            {

                stats["enemyBaseWithinRange"] = false;
            }
        }
        else
        {

            stats["enemyBaseWithinRange"] = false;
        }
    }


    void lostSight()
    {
        if (backWardsChain("searchState", backwardsChainFromSearchToChaseConditions, Rule.Predicate.Or))  // use backwards chaning to figure out if we lost the enemy during chase are not 
        {

            stats["lostSight"] = true;


            chaseTime = 0.0f;
        }

        if (stats["lostSight"] == true)
        {
            chaseTime += Time.deltaTime;//  increment chase timer while he dont know where the enemy is 
            if (stats["chaseState"] == false) //
            {
                Debug.Log("MoveToTarget is true");
                stats["moveToTarget"] = true;
            }
            else
            {
                stats["moveToTarget"] = false;
            }

            if (chaseTime > chaseTimeMax)
            {
                stats["chaseTimer"] = true;

            }
            // if we have reached our chase time or the position we are moving to our the rule dictionary or the rule dictionary check is hit 
            if (stats["chaseTimer"] == true || stats["shouldChase"] == true || stats["shouldRetreat"] == true
                || Vector3.Distance(transform.position, LastKnownEPos.transform.position) < chaseDistanceCheck)
            {
                stats["wasWithinRange"] = false;
                stats["moveToTarget"] = false;
                stats["ChaseTimer"] = false;
                stats["lostSight"] = false;
                stats["hadChase"] = false;

                chaseTime = 0.0f;


            }

        }





    }



    private bool backWardsChain(string consequenceState, List<Dictionary<string, bool>> conditionsToCheck, Rule.Predicate condtionComparison)
    {

        if (stats[consequenceState])
        {
            int booleanResult = 0;

            foreach (Dictionary<string, bool> condition in conditionsToCheck)
            {

                booleanResult += Convert.ToInt32(checkSum(condition));

            }
            switch (condtionComparison)
            {

                case Rule.Predicate.Or:
                    {
                        return booleanResult > 0;
                    }
                case Rule.Predicate.And:
                    {
                        return booleanResult == conditionsToCheck.Count - 1;

                    }


            }


        }
        return false;




    }


    public void shouldSearch()
    {
        if (stats["searchState"] == false && stats["attackState"] == false && stats["chaseState"] == false && stats["shouldRetreat"] == false)
        {

            stats["search"] = false;
        }
        else
        {
            stats["search"] = true;

        }
    }
    private void initStateMachine()
    {

        Dictionary<Type, BaseST> states = new Dictionary<Type, BaseST>
        {
            {typeof(CC_SearchStateRBS),new CC_SearchStateRBS(this)},
            {typeof(CC_AttackStateRBS),new CC_AttackStateRBS(this)},
            {typeof(CC_RetreatStateRBS),new CC_RetreatStateRBS(this)},
            {typeof(CC_ChaseStateRBS),new CC_ChaseStateRBS(this)}
        };

        if (!TryGetComponent(out CC_smartTankFSMRBSBT bt))
        {
            GetComponent<CC_FSM>().setStates(states);
        }



    }



    private bool checkSum(Dictionary<string, bool> statsList) // utility function to check multiple stats against the global stat dictionary  at once
    {

        foreach (KeyValuePair<string, bool> stat in statsList)
        {

            if (stat.Value != stats[stat.Key])
            {

                return false;
            }


        }
        return true;

    }





    public bool BtStatMultiQuery(string statsName)
    {


        return checkSumbt(multiStatQueryForBT[statsName]);



    }
    private bool checkSumbt(Dictionary<string, bool> statsList) // utility function to check multiple stats against the global stat dictionary  at once
    {

        foreach (KeyValuePair<string, bool> stat in statsList)
        {

            if (stat.Key.Contains("State"))
            {
                Debug.Log("ommitted " + stat.Key);
                continue;
            }

            if (stat.Value != stats[stat.Key])
            {

                return false;
            }


        }
        return true;

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

        SetEnemySeen();
        checkAmmo();
        CheckFuel();
        CheckHealth();
        IsWithinRange();
        CheckShouldRetreat();
        SetEnemyBaseSeen();
        CheckShouldChase();
        CheckCanAttack();
        lostSight();
        enemyBaseWithinRange();
        CheckCanAttackEnemyBase();
        CheckChaseBase();
    }
}