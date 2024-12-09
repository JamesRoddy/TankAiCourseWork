using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        stats.Add("attackEnemyBase", false);
        stats.Add("enemyBaseWithinRange", false);
        stats.Add("chaseEnemy", false);
    }

    public void InitiliseRules()  
    {
        rules.addRule(new Rule("shouldRetreat", "attackState", typeof(RetreatRBS), Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat
        rules.addRule(new Rule("shouldRetreat", "searchState", typeof(RetreatRBS), Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat
        rules.addRule(new Rule("shouldRetreat", "chaseState", typeof(RetreatRBS), Rule.Predicate.And)); // if we see the enemy and are on low health then we should retreat
        rules.addRule(new Rule("enemyBaseWithinRange", "canAttack", typeof(CC_AttackStateRBS), Rule.Predicate.And));
        rules.addRule(new Rule("withinRange", "canAttack", typeof(CC_AttackStateRBS), Rule.Predicate.And));// if we are able to attack(our health and fuel are high and ammo isn't a major priority) we should go into the attack state
        rules.addRule(new Rule("shouldChase", "canAttack", typeof(ChaseRBS), Rule.Predicate.And)); // if we are in the attack state
        rules.addRule(new Rule("attackState", "chaseEnemy", typeof(ChaseRBS), Rule.Predicate.Or));
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
        if (priorityManager.checkLow(PRIORITIES.FUEL)  == true) // checks if we are low on fuel
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
            Debug.Log("lowAmmo");
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
            Debug.Log("enemySeen is true");
            stats["enemySeen"] = true;
        }
        else
        {
            Debug.Log("enemySeen is false");
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
        if (stats["enemySeen"] == true)
        {
          if (Vector3.Distance(enemyTank.transform.position, transform.position) < TankFiringDistance
               && !priorityManager.checkQueue(queuePriority.CRITICAL, PRIORITIES.AMMO))
          {
                Debug.Log("enemy is in range");
                stats["withinRange"] = true;
          }
          else
          {
                stats["withinRange"] = false;
          }
        }

/*        if (stats["enemyBaseSeen"] == true)
        {
            if (Vector3.Distance(transform.position, EnemyBasePos.transform.position) < BaseFiringDistance
                )
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
            Debug.Log("enemy is not range");
            stats["withinRange"] = false;
        }*/
    }

    public void CheckCanAttack()
    {
        if(    stats["highHealth"] == true
            && stats["ammoCritical"] == false
            && stats["shouldRetreat"] == false
            && stats["attackState"] == false)
            {
                stats["canAttack"] = true;
                Debug.Log("canAttack is true ");
            }
        if (stats["highHealth"] == true
            && stats["ammoCritical"] == false
            && stats["shouldRetreat"] == false
            && stats["attackState"] == false)
        {
            Debug.Log("canAttack is true ");
            stats["canAttack"] = true;
        }

        else
        {
         stats["canAttack"] = false;
         Debug.Log("canAttack is false");
        }
    }

    public void CheckShouldRetreat()
    {
        if (stats["lowHealth"] == true && stats["enemySeen"] == true && stats["retreatState"] == false)
        {
        //    Debug.Log("shouldRetreat is true ");
            stats["shouldRetreat"] = true;
        }
        else
        {
            stats["shouldRetreat"] = false;
        }
    }
    public void CheckShouldChase()
    {
        if (stats["enemySeen"] == true || stats["enemyBaseSeen"] == true)
        {
            Debug.Log("enemySeen or EnemyBaseSeen is true");
            if (stats["highFuel"] == true && stats["highHealth"] == true)
            {
                if (//stats["withinRange"] == false
                    stats["ammoCritical"] == false
                   && stats["chaseState"] == false
                   /*&& stats["attackState"] == false*/)
                {
                    Debug.Log("should chase is true");
                    stats["shouldChase"] = true;
                }

                if (//stats["enemyBaseWithinRange"] == false
                   stats["ammoCritical"] == false
                  && stats["chaseState"] == false
                  /*&& stats["attackState"] == false*/)
                {
                    Debug.Log("should chase is true");
                    stats["shouldChase"] = true;
                }

                else
                {
                    Debug.Log("should chase is false");
                    stats["shouldChase"] = false;
                }
            }
/*            else if (stats["chaseState"] == false)
            {
                Debug.Log("should chase is false");
                stats["shouldChase"] = false;
            }*/

        }
        else
        {
            Debug.Log("should chase is false");
            stats["shouldChase"] = false;
        }
    }

    public void AttackEnemyBase()
    {
        if (stats["ammoCritical"] == false
            && stats["attackState"] == false)
        {
            stats["attackEnemyBase"] = true;
        }
        else
        {
            stats["attackEnemyBase"] = false;
        }
    }

    public void enemyBaseWithinRange()
        {
            if (stats["enemyBaseSeen"] == true)
            {
                if (Vector3.Distance(transform.position, EnemyBasePos.transform.position) < BaseFiringDistance)
                {
                    Debug.Log("enemy base is in range");
                    stats["enemyBaseWithinRange"] = true;
                }
                else
                {
                    Debug.Log("enemy base is not in range");
                    stats["enemyBaseWithinRange"] = false;
                }
            }
            else if (stats["enemyBaseSeen"] == false)
            {
                Debug.Log("enemy base is not in range");
                stats["enemyBaseWithinRange"] = false;
            }
    }

    public void ChaseEnemy()
    {
        if (stats["enemySeen"] == true)
        {
            if (stats["highFuel"] == true && stats["highHealth"] == true)
            {
                Debug.Log("priorities hit to chase ");


                if (Vector3.Distance(transform.position, LastKnownEPos.transform.position) > TankFiringDistance
                    && stats["ammoCritical"] == false)
                {
                    Debug.Log("chaseEnemy is true");
                    stats["chaseEnemy"] = true;

                }
                else
                {
                    Debug.Log("chaseEnemy is false");
                    stats["chaseEnemy"] = false;
                }
            }
            else
            {
                Debug.Log("chaseEnemy is false");
                stats["chaseEnemy"] = false;
            }
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