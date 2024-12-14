using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;
using static BTaction;
using UnityEngine.Video;
public class CC_smartTankFSMRBSBT : CC_SmartTankRBS
{


    

    public BTaction targetVisisbleCheck;
    public BTaction withinRangeCheck;
    public BTaction checkHighHealth;
    public BTaction checkLowHealth;
    public BTaction checkHighAmmo;
    public BTaction checkLowAmmo;
    public BTaction checkAmmoCritical;
    public BTaction checkSearch;
    public BTaction checkHighFuel;
    public BTaction checkLowFuel;
    public BTaction checkShoulChaseBase;
    public BTaction checkShouldChase;
    public BTaction checkLostTarget;
    public BTaction checkShouldAttack;
    public BTaction checkShouldRetreat;
    public BTaction checkShouldAttackBase;
    public BTaction checkEnemyBaseVisible;
    public BTaction checkEnemyVisible;

    public BTaction checkWihtinRangeOfBase;
    public BTaction seenConsumable;
    public BTaction priorityCheck;

    public BTaction attackBase;
    public BTaction attackEnemy;


    BTaction checkEnemyPos;
    BTaction lookForEnemy;
    BTaction getSafeSpot;
    BTaction moveToSafeSpot;


    
   public BTselector attackingBase;
   public BTselector attackingEnemy;
   public BTsequence retreating;
   public List<BTsequence> sequencesFromSearch ;
   public List<BTselector> selectorsFromSearch;
    public List<BTselector> selectorsForAttack;
   public BTsequence chasing;

   public BTaction checkHighPriority; 
   public BTaction checkLowPriority;
   public BTsequence resourceCheck;
   public BTsequence evaluateConsumable;
   public BTaction moveToconsumable;

    private BTAttackActions tankActionsForAttack;
    private BTActionsSearch tankActionsForSearch;
    private BTActionsChase tankActionsForChase;
    private BTactionRetreat tankActionsForRetreat;
    private List<queuePriority> priorityOrder =  new List<queuePriority> { queuePriority.MAJOR, queuePriority.CRITICAL };
   public List<PRIORITIES> currentPriorityResources = new List<PRIORITIES>();


    List<Rule> rulesForFSMRBSBT;


    // timers associated with certain actions
    private CC_BTFSMRBChaseState chaseDebugs;
    private CC_smartTankFSMRBSBT debugTanks;
    private BTFSMRBSAttack attackDebugs;
    private CC_BTFSMRBSRetreatState retreatDebugs;
    private CC_BTFSMRBSSearchState searchDebugs;
    
    float attackBaseTimer = 2.0f;



    private  void Awake()
    {

        debugTanks = new CC_smartTankFSMRBSBT();
        chaseDebugs = new CC_BTFSMRBChaseState(debugTanks);
        attackDebugs = new BTFSMRBSAttack(debugTanks);
        retreatDebugs = new CC_BTFSMRBSRetreatState(debugTanks);
        searchDebugs = new CC_BTFSMRBSSearchState(debugTanks);
       
        
        initStateMachine();
       
    }

    public BTNODESTATES checkShouldSearch()
    {

        if (stats["searchState"])
        {
            Debug.Log("searchState");
            tankActionsForSearch.search();
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }

    }


    public BTNODESTATES findPriorityConsuamble()
    {

        currentPriorityResources = priorityManager.sweepQueues(priorityOrder);
        
        if(currentPriorityResources.Count>0  )
        {
            if (currentPriorityResources.Contains(PRIORITIES.HEALTH))
            {
                
            }
            
            return BTNODESTATES.FAILURE;
        }
       
        else
        {
            return BTNODESTATES.SUCCESS;
        }



    }
    public BTNODESTATES ActionSeeConsumable ()
    {

        if (consumablesFound.Count > 0)
        { 
            return BTNODESTATES.FAILURE;
            
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }



    }
    public BTNODESTATES checkHasPriorityresource()
    {

        currentPriorityResources = priorityManager.sweepQueues(priorityOrder);
        currentPriorityResources = priorityManager.sweepQueues(priorityOrder);

        if (currentPriorityResources.Count > 0)
        {
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }





    }


    public BTNODESTATES checkPosRetreat()
    {

        if (!tankActionsForRetreat.checkEtankPos()) // certain node states are based on the success of actions that need to be performed by the tank istelf 
        {
            Debug.Log("checming position");
            // for exmaple here the checkEpos method inside the retreat actions wrapper will return true if the wait time for checkin the position is reached or if the enemy tank was seen 
            return BTNODESTATES.FAILURE;// return failure if we havent seen the enemy tank yet or while we havent reached our wait time 
        }
        else if (enemyTank == null) // if we ddint see the enemy tank that means the retreat was successful and we are safe so the sequence is 'forced' inot success through the force success state for nodes 
        {
            Debug.Log("found  no tank in retreat");

            return BTNODESTATES.FORCESUCCES; // consider the whole sequence complete 
        }
        else
        {   
            Debug.Log("found tank continue retreat  sequence");
            return BTNODESTATES.SUCCESS; // otherwise if we saw the enemy tank we need to continue the sequence regardless 
        }
    }


    public BTNODESTATES getSafteySpot()
    {

        if (tankActionsForRetreat.findSafteySpot())// know if we found a succesful safety spot 
        {
            Debug.Log("finding  spot retreat sequence");

            return BTNODESTATES.SUCCESS;
        }
        else
        {
            Debug.Log("finding  spot failied checking for position ref ");

            return BTNODESTATES.FAILURE;// if not go back to waiting to get a ref position in relation to the enemy 
        }
    }
    public BTNODESTATES moveToSaftey()
    {

        if (tankActionsForRetreat.running()) // if we are still moving to the saftey spot then reset the retreat sequence 
        {

            Debug.Log("moving to safe spot sequence");

            return BTNODESTATES.FAILURE;
        }
        else
        {
            Debug.Log("reached safe spot repeating sequence");

            return BTNODESTATES.REPEAT; // this node is marked a repeat as when we reach our saftey spot we
                                        // will need to loop back to the beggining in order to check if the enemy is
                                        // behind us and since we have a node that forces success of the
                                        // sequence when we dont see the enemy after running we dont need to
                                        // mark this final part of the chain as success just that it should repeat
        }



    }

    public BTNODESTATES ActionCheckBaseRange()
    {

        if (stats["enemyBaseWithinRange"])
        {
            
            tankActionsForAttack.AttackBase();
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }


    }
    public BTNODESTATES ActionCheckRange()
    {
 
            if (stats["withinRange"])
            {
            Debug.Log("wihtin range check ");
            Debug.Log("within range btfsmrbs");
                tankActionsForAttack.attackEnemy();
                return BTNODESTATES.FAILURE;
            }
            else
            {
                return BTNODESTATES.SUCCESS;
            }
        
        


    }
    public BTNODESTATES ActionCheckHighFuel()
    {

        if (stats["highFuel"])
        {
        

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }


    }
    public BTNODESTATES ActionCheckHighAmmo()
    {

        if (stats["highAmmo"])
        {

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }




    }

    public BTNODESTATES ActionCheckEnemyBaseVisisble()
    {

        if (stats["enemyBaseSeen"])
        {
           

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }

    }

    public BTNODESTATES ActionCheckEnemyVisisble()
    {

        if (stats["enemySeen"])
        {
          
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;


        }

    }

    
    public BTNODESTATES ActionCheckCanAttackBase()
    {
        if (BtStatMultiQuery("canAttackBase"))
        {


            return BTNODESTATES.FAILURE;

        }
        else
        {

            return BTNODESTATES.SUCCESS;
        }


    }

    public BTNODESTATES ChaseTargetNotVisisble()
    {

        if (stats["lostSight"])
        {
        
            tankActionsForChase.moveToLastKnownPos();
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }
    }

    public BTNODESTATES ActionCheckHighHealth()
    {

        if (stats["highHealth"])
        {
           

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }




    }

    public BTNODESTATES ActionCheckLowFuel()
    {

        if (stats["lowFuel"])
        {
           

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }




    }
    public BTNODESTATES ActionCheckLowAmmo()
    {

        if (stats["lowAmmo"])
        {
           

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;

        }




    }


    public BTNODESTATES chaseBase()
    {
        if (BtStatMultiQuery("chaseBase"))
        {
        
            tankActionsForChase.moveToBase();
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }

    }
    public BTNODESTATES ActionCheckShouldChase()
    {

        if (BtStatMultiQuery("shouldChase"))
        {
        

            tankActionsForChase.moveToKnownEnemyPos();
            return BTNODESTATES.FAILURE;



        }
        else
        {
            return BTNODESTATES.SUCCESS;

        }




    }


    public BTNODESTATES ActionCheckLowHealth()
    {

        if (stats["lowHealth"])
        {
       

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }




    }


    public BTNODESTATES ActionCanAttackEnemyBase()
    {

        if (BtStatMultiQuery("canAttackBase"))
        {
           
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;


        }

    }


    public BTNODESTATES ActionCanAttackEnemy()
    {
        
        if (BtStatMultiQuery("canAttack"))
        {
            Debug.Log("CAN ATTACK");
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }

    }

    public BTNODESTATES ActionCheckLostSight()
    {

        if (stats["lostSight"])
        {
         
            tankActionsForChase.moveToLastKnownPos();
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }



    }

    
    public BTNODESTATES ActionCheckShouldRetreat()
    {
        if (BtStatMultiQuery("shouldRetreat"))
        {
            

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }
    }

    public BTNODESTATES ActionmCheckCriticalAmmo()
    {

        if (stats["ammoCritical"])
        {
           

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }




    }

    private void initStateMachine()
    {


        Dictionary<Type, BaseST> states = new Dictionary<Type, BaseST>
        {
           
            {typeof(CC_BTFSMRBSSearchState),new CC_BTFSMRBSSearchState(this)},
            {typeof(BTFSMRBSAttack),new BTFSMRBSAttack(this)},
            {typeof(CC_BTFSMRBChaseState),new CC_BTFSMRBChaseState(this)},
            {typeof(CC_BTFSMRBSRetreatState), new CC_BTFSMRBSRetreatState(this) }

        };
        foreach (var state in states) {
        
          Debug.Log("states "+ state.Key);
        
        }
        GetComponent<CC_FSM>().setStates(states);




    }


    public bool evaluateSelectors(List<BTselector> selectors) // util function to evaluate multiple sequences at once
    {
     
        BTNODESTATES result = BTNODESTATES.FAILURE;
        foreach (BTselector selector in selectors)
        {
            

            result = selector.evaluate();
             if(result == BTNODESTATES.FAILURE)
             {
                
                return true;
             }

        }

       
            return false; // no selector needed execution 

    }



    public bool evaluateSequences(List<BTsequence> sequences) // allows us to evaluate multiple seuqences at once to se if we should go into other states from search for exmaple
    {
        
        foreach (BTsequence sequence in sequences)
        {
          if(sequence.evaluate() == BTNODESTATES.FAILURE) {

                return true; 
          }
        }
       
        return false; // no seqeuence needed execution 

    }

    public override void AITankStart()
    {
        base.AITankStart();

        InitiliseStats();
        rulesForFSMRBSBT = new List<Rule>
        {
         new Rule("shouldRetreat", "attackState", typeof(CC_BTFSMRBSRetreatState),retreatDebugs, Rule.Predicate.And), // if we see the enemy and are on low health then we should retreat
        new Rule("shouldRetreat", "searchState", typeof(CC_BTFSMRBSRetreatState), retreatDebugs,Rule.Predicate.And), // if we see the enemy and are on low health then we should retreat
        new Rule("shouldRetreat", "chaseState", typeof(CC_BTFSMRBSRetreatState),retreatDebugs, Rule.Predicate.And), // if we see the enemy and are on low health then we should retreat
        new Rule("withinRange", "canAttack", typeof(BTFSMRBSAttack),attackDebugs, Rule.Predicate.And),// if we are able to attack(our health and fuel are high and ammo isn't a major priority) we should go into the attack state
        new Rule("enemyBaseWithinRange", "canAttackBase", typeof(BTFSMRBSAttack), attackDebugs, Rule.Predicate.And),
        new Rule("shouldChase", "moveToTarget", typeof(CC_BTFSMRBChaseState),chaseDebugs ,Rule.Predicate.Or), // if we are in the attack state
        new Rule("enemyBaseSeen", "chaseBase", typeof(CC_BTFSMRBChaseState), chaseDebugs, Rule.Predicate.And),

        };
        InitiliseRules(rulesForFSMRBSBT);
        foreach (Rule rule in rules.GetRules)
        {
            Debug.Log("rule type " + rule.debugType.GetType());
        }
        initRuleDictionaries();
        tankActionsForAttack = new BTAttackActions(this);
        tankActionsForSearch = new BTActionsSearch(this);
        tankActionsForChase = new BTActionsChase(this);
        tankActionsForRetreat = new BTactionRetreat(this);

        checkEnemyPos = new BTaction(checkPosRetreat);
        getSafeSpot = new BTaction(getSafteySpot);
        moveToSafeSpot = new BTaction(moveToSaftey);


        targetVisisbleCheck = new BTaction(ActionCheckEnemyVisisble);
        withinRangeCheck = new BTaction(ActionCheckRange);
        checkHighHealth = new BTaction(ActionCheckHighHealth);
        checkLowHealth = new BTaction(ActionCheckLowHealth);
        checkHighAmmo = new BTaction(ActionCheckHighAmmo);
        checkLowAmmo = new BTaction(ActionCheckLowAmmo);
        checkAmmoCritical = new BTaction(ActionmCheckCriticalAmmo);




        checkHighFuel = new BTaction(ActionCheckHighFuel);
        checkLowFuel = new BTaction(ActionCheckLowFuel);

        checkShouldChase = new BTaction(ActionCheckShouldChase);
        checkLostTarget = new BTaction(ActionCheckLostSight);
        checkShoulChaseBase = new BTaction(chaseBase);
        checkSearch = new BTaction(checkShouldSearch);
        checkShouldAttack = new BTaction(ActionCanAttackEnemy);
        checkShouldRetreat = new BTaction(ActionCheckShouldRetreat);
        checkShouldAttackBase = new BTaction(ActionCanAttackEnemyBase);
        checkEnemyBaseVisible = new BTaction(ActionCheckEnemyVisisble);
        checkWihtinRangeOfBase = new BTaction(ActionCheckBaseRange);


        priorityCheck = new BTaction(checkHasPriorityresource);
        seenConsumable = new BTaction(ActionSeeConsumable);


        attackingEnemy = new BTselector(new List<BTbaseNode> { checkShouldAttack, withinRangeCheck });
        attackingBase = new BTselector(new List<BTbaseNode> { checkShouldAttackBase, checkWihtinRangeOfBase });
        chasing = new BTsequence(new List<BTbaseNode> { checkLostTarget, checkShouldChase, checkShoulChaseBase });

        retreating = new BTsequence(new List<BTbaseNode> { checkEnemyPos, getSafeSpot, moveToSafeSpot });


        sequencesFromSearch = new List<BTsequence> { chasing }; // allows us to evelaute multiple seuqences at once to se if we should go into other states from search for exmaple
        selectorsFromSearch = new List<BTselector> { attackingEnemy, attackingBase }; // same as above but with selectors
        selectorsForAttack = new List<BTselector> { attackingEnemy, attackingBase };
        evaluateConsumable = new BTsequence(new List<BTbaseNode> { seenConsumable, priorityCheck });












    }

    public override void AITankUpdate()
    {
        base.AITankUpdate();







    }

    public override void AIOnCollisionEnter(Collision collision)
    {
        base.AIOnCollisionEnter(collision);




    }



















}
