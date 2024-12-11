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
    public float currentSpeed = 0.85f;
    public Dictionary<string, bool> stats = new Dictionary<string, bool>();
    public Rules rules = new Rules();
    public float chaseTime = 0.0f;
    public float chaseTimeMax = 2.0f;
    ChaseRBS chaseDebug;
    CC_SmartTankRBS debugTank;
    CC_AttackStateRBS attackDebug;
    RetreatRBS retreatDebug;
    SearchStateRBS searchDebug; 
/*    Dictionary<string, bool> chaseEnemyCheck = new Dictionary<string, bool>
    {
        {"enemySeen",false},
        {"chaseState",true},
        {"highFuel",true},
        {"highHealth",true},

    };*/
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
/*    Dictionary<string, bool> shouldChaseCheck = new Dictionary<string, bool>
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

    };*/
    Dictionary<string, bool> shouldChaseTargetNotVisible = new Dictionary<string, bool>
    {
        {"wasWithinRange",true },
        {"chaseTimer",false },
        {"enemyBaseSeen",false },
        { "enemySeen",false },
        { "highFuel",true },
        { "ammoCritical",false },
        { "highHealth", true },
        {"chaseState",false },

    };



    Dictionary<string, bool> shouldRetreatCheck = new Dictionary<string, bool>
    {
        { "lowHealth",true },
        { "enemySeen",true },
        { "retreatState",false }
    };

    private void Awake()
    {
        debugTank = new CC_SmartTankRBS();
        chaseDebug = new ChaseRBS(debugTank);
        attackDebug = new CC_AttackStateRBS(debugTank);
        retreatDebug = new RetreatRBS(debugTank); 
        searchDebug = new SearchStateRBS(debugTank);
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
        stats.Add("ammoMajor", false);
        stats.Add("fuelMajor", false);
        stats.Add("healthMajor", false);
        stats.Add("enemyBaseWithinRange", false);
        stats.Add("chaseEnemy", false);
        stats.Add("chaseTimer", false);
        stats.Add("search", false);
        stats.Add("moveToTarget", false);
        stats.Add("lostSight", false);
        stats.Add("wasWithinRange", false);
     /*   stats.Add("shootBase", false);*/
       
     /*   stats.Add("shootTank", false);
        stats.Add("shootBase", false);*/

    }

    public void InitiliseRules()
    {
        
        rules.addRule(new Rule("shouldRetreat", "attackState", typeof(RetreatRBS),retreatDebug, Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat


/*        rules.addRule(new Rule("shouldRetreat", "searchState", typeof(RetreatRBS), Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat
        rules.addRule(new Rule("shouldRetreat", "chaseState", typeof(RetreatRBS), Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat
    rules.addRule(new Rule("enemyBaseWithinRange", "canAttack", typeof(CC_AttackStateRBS), Rule.Predicate.And));*/
        rules.addRule(new Rule("withinRange", "canAttack", typeof(CC_AttackStateRBS),attackDebug, Rule.Predicate.And));// if we are able to attack(our health and fuel are high and ammo isn't a major priority) we should go into the attack state
        rules.addRule(new Rule("shouldChase", "moveToTarget", typeof(ChaseRBS),chaseDebug ,Rule.Predicate.Or)); // if we are in the attack state
    }






    public void CheckSpeed()
    {
        if (stats["searchState"] == true && stats["highFuel"] == true)
        {
            currentSpeed = 0.85f;
        }
        else if (stats["searchState"] == true && stats["lowFuel"])
        {
            currentSpeed = 0.5f;
        }
        else if (consumablesFound.Count > 0)
        {
            currentSpeed = 1.0f;
        }
        else if (stats["retreatState"] == true)
        {
            currentSpeed = 1;
        }

        else if (stats["chaseState"] == true)
        {
            currentSpeed = 1;
        }
       
        
    }
    public void CheckHealth()
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

    public void CheckFuel()
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
            stats["highFuel"] = true; // if yes then return as true

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

    public void checkAmmo()
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

    public void SetEnemyBaseSeen()
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

    public void IsWithinRange()
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

    public void CanAttackBase()
    {

        if (stats["enemyBaseSeen"] == true && stats["enemyBaseWithinRange"] == true)
        {
            stats["canAttack"] = true;
        }
        else
        {
            stats["canAttack"] = false;

        }

    }
    public void CheckCanAttack()
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

    public void CheckCanAttackEnemyBase()
    {
    }

    public void CheckShouldRetreat()
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
    public void CheckShouldChase()
    {
        if (stats["chaseState"] == false )
        {
            Debug.Log("chase state false " + stats["chaseState"]);
            if (checkSum(shouldChaseBaseCheck) )
            {
               

                Debug.Log("chasing enemy without timer ");
                stats["shouldChase"] = true;
                return;
            }
          

        }
        
        stats["shouldChase"] = false;




    }

    // as search is the defualt state to return to this method will be called in the enrty method for any timers governed by the rules that switch into search
    public void resetTimersIntoSearch()
    {
        
            Debug.Log("switch to search");
       
        
    }
    public void enemyBaseWithinRange()
    {
        if (stats["enemyBaseSeen"] == true)
        {
            if (Vector3.Distance(transform.position, EnemyBasePos.transform.position) < BaseFiringDistance)
            {
              
                stats["enemyBaseWithinRange"] = true;
            }
            else
            {
             
                stats["enemyBaseWithinRange"] = false;
            }
        }
        else if (stats["enemyBaseSeen"] == false)
        {
            
            stats["enemyBaseWithinRange"] = false;
        }
    }

    public bool timerGuard(float timer )
    {
        return timer > 0.0f;
    }
    void lostSight()
    {
        Debug.Log("CHASE STATE iS tUre" + stats["chaseState"]);
        Debug.Log("ATTTACK STATE iS tUre" + stats["chaseState"]);
        Debug.Log("cHEK sum For chAse When target n v" + checkSum(shouldChaseTargetNotVisible));
        if ((stats["searchState"] ) && checkSum(shouldChaseTargetNotVisible)) // use backwards chaning to figure out if we loots the enemy during chase are not 
        {
            Debug.Log("lost sIgHt");
           stats["lostSight"] = true;
           chaseTime = 0.0f;
        }

        if (stats["lostSight"] == true  )
        {
            chaseTime += Time.deltaTime;
            if (stats["chaseState"] == false) 
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
                Debug.Log("chase timer is true");
                stats["chaseTimer"] = true;

            }
            Debug.Log("current bool for chase time" +chaseTime);
        
            Debug.Log("move to target bool chase " + stats["moveToTarget"]);

            Debug.Log("chasing with timer to attack enemy " + chaseTime);
          

        }
        if (stats["chaseTimer"] == true || stats["shouldChase"] == true)
        {
            Debug.Log("chase timer reset" + stats["chaseTimer"]);
            stats["wasWithinRange"] = false;
            stats["moveToTarget"] = false;
            stats["ChaseTimer"] = false;
            stats["lostSight"] = false;
            chaseTime = 0.0f;


        }




    }

    public void ChaseEnemy()
    {

     /*   if (checkSum(chaseEnemyCheck) == true)
        {
            if (stats["enemySeen"] == true)
            {
                

                stats["shouldChase"] = true;
                t = 0;

            }
            else
            {
                if (t < timer)
                {
                    t += Time.deltaTime;

                    Debug.Log("chasing with timer RBS, t is " + t);
                    stats["chaseEnemy"] = true;
                    Debug.Log("chaseEnemy");
                    
                }

                else if (t >= timer)
                {
                    Debug.Log("chase timer was reached t is " + t);
                    stats["chaseEnemy"] = false;

                    t = 0;
                }
            }
        }*/
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
            {typeof(SearchStateRBS),new SearchStateRBS(this)},
            {typeof(CC_AttackStateRBS),new CC_AttackStateRBS(this)},
            {typeof(RetreatRBS),new RetreatRBS(this)},
            {typeof(ChaseRBS),new ChaseRBS(this)}
        };


        GetComponent<CC_FSM>().setStates(states);


    }



    private bool checkSum(Dictionary<string, bool> statsList) // utility function to check multiple stats at once
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

    public void CheckRules()
    {
        foreach (var item in rules.GetRules) // iterates through the rules
        {


            Debug.Log("current rule being checked is" + item.debugType.GetType());
            Debug.Log(item.debugType.GetType() + "antecedent a is " + item.antecentA + " is " + stats[item.antecentA] + " antecedent b is " + item.antecentB + " is " + stats[item.antecentB]);
            if (item.CheckRule(stats) != null) // if a rule doesn't return null
            {
                Debug.Log("rule fired " + item.debugType.GetType());
            }
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

        

        SetEnemySeen();
        checkAmmo();
        CheckFuel();
        CheckHealth();
        IsWithinRange();
        CheckShouldRetreat();
        SetEnemyBaseSeen();
        CheckSpeed();
        enemyBaseWithinRange();
        CheckShouldChase();
        CheckCanAttack();
        lostSight();
        /*CanAttackBase();*/


        /*shouldSearch();*/
        /*       CheckRules();*/
    }
}