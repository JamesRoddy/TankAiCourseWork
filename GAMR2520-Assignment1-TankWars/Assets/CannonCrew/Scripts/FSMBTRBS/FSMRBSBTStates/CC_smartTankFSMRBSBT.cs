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
    
    
   public BTselector attackingBase;
   public BTselector attackingEnemy;
   public BTsequence retreating;
   public List<BTsequence> sequencesFromSearch ;
   public List<BTselector> selectorsFromSearch;
   public BTsequence chasingEnemy;

   public BTaction checkHighPriority; 
   public BTaction checkLowPriority;
   public BTsequence resourceCheck;
    public BTsequence evaluateConsumable;
   public BTaction moveToconsumable;

    private BTAttackActions tankActionsForAttack;
    private BTActionsSearch tankActionsForSearch;
    private BTActionsChase tankActionsForChase;
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
        
        InitiliseStats();
        InitiliseRules(rulesForFSMRBSBT);
        foreach(Rule rule in rules.GetRules)
        {
            Debug.Log("rule type "+rule.debugType.GetType());
        }
        initRuleDictionaries();
        initBt();
        initStateMachine();
       
    }







    private void initBt()
    {

        tankActionsForAttack = new BTAttackActions(this);
        tankActionsForSearch = new BTActionsSearch(this);
        tankActionsForChase = new BTActionsChase(this);


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
        checkSearch = new BTaction(checkShouldSearch);
        checkShouldAttack = new BTaction(ActionCanAttackEnemy);
        checkShouldRetreat = new BTaction(ActionCheckShouldRetreat);
        checkShouldAttackBase = new BTaction(ActionCanAttackEnemyBase);
        checkEnemyBaseVisible = new BTaction(ActionCheckEnemyVisisble);
        checkEnemyVisible = new BTaction(ActionCheckEnemyBaseVisisble);
        checkWihtinRangeOfBase = new BTaction(ActionCheckBaseRange);
        

        priorityCheck = new BTaction(checkHasPriorityresource);
        seenConsumable = new BTaction(ActionSeeConsumable);

        
        attackingEnemy = new BTselector(new List<BTaction> {  checkShouldAttack, withinRangeCheck });
        attackingBase = new BTselector(new List<BTaction> { checkShouldAttackBase, checkWihtinRangeOfBase });
        chasingEnemy = new BTsequence(new List<BTaction> { checkLostTarget, checkShouldChase });
        retreating = new BTsequence(new List<BTaction> { checkShouldRetreat });
        sequencesFromSearch = new List<BTsequence> { chasingEnemy }; // allows us to evelaute multiple seuqences at once to se if we should go into other states from search for exmaple
        selectorsFromSearch = new List<BTselector> {attackingEnemy }; // same as above but with selectors
        evaluateConsumable = new BTsequence(new List<BTaction> { seenConsumable, priorityCheck });







    }


    public BTNODESTATES checkShouldSearch()
    {
        
        if (stats["searchState"])
        {
            tankActionsForSearch.search();
            Debug.Log("searching state for fsmRbSBt");
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

    public BTNODESTATES ActionCheckBaseRange()
    {

        if (stats["enemyBaseWithinRange"])
        {
            Debug.Log(" enemyBaseWithinRange btfsmrbs");
            
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
        Debug.Log("check range fsmrbsbt");
            if (stats["withinRange"])
            {
                Debug.Log(" withinRange btfsmrbs");

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
            Debug.Log(" highFuel btfsmrbs");

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
            Debug.Log(" highAmmo btfsmrbs");

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
            Debug.Log(" enemyBaseSeen btfsmrbs");

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
            Debug.Log(" enemySeen btfsmrbs");

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;


        }

    }
    public BTNODESTATES isNotInRangeOfBase()
    {
        if (!stats["enemyBaseWithinRange"])
        { 

            Debug.Log(" enemyBaseWithinRange btfsmrbs");

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }
    }
    public BTNODESTATES ActionCheckCanAttackBase()
    {
        if (stats["canAttackBase"])
        {
            Debug.Log(" canAttackBase btfsmrbs");

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
            Debug.Log(" lostSight btfsmrbs");
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
            Debug.Log(" highHealth btfsmrbs");

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
            Debug.Log(" lowFuel btfsmrbs");

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
            Debug.Log(" lowAmmo btfsmrbs");

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;

        }




    }



    public BTNODESTATES ActionCheckShouldChase()
    {

        if (stats["shouldChase"])
        {
            Debug.Log(" shouldChase btfsmrbs");

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
            Debug.Log(" lowHealth btfsmrbs");

            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }




    }


    public BTNODESTATES ActionCanAttackEnemyBase()
    {

        if (stats["canAttackBase"])
        {
            Debug.Log("can attack base btfsmrbs");
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;


        }

    }


    public BTNODESTATES ActionCanAttackEnemy()
    {
        
        if (stats["canAttack"])
        {
            Debug.Log("can attack btfsmrbs");
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
            Debug.Log(" lostSight btfsmrbs");
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
        if (stats["shouldRetreat"])
        {
            Debug.Log(" shouldRetreat btfsmrbs");

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
            Debug.Log(" ammoCritical btfsmrbs");

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


        };

        GetComponent<CC_FSM>().setStates(states);




    }


    public bool evaluateSelectors(List<BTselector> selectors) // util function to evaluate multiple sequences at once
    {
        Debug.Log("selectors check");
        BTNODESTATES result = BTNODESTATES.FAILURE;
        foreach (BTselector selector in selectors)
         {
            result = selector.evaluate();
             

        }

        if(result != BTNODESTATES.SUCCESS)
        {
            return true;
        }
        return false; // no selector needed execution 

    }



    public bool evaluateSequences(List<BTsequence> sequences) // util function to evaluate multiple sequences at once
    {
        Debug.Log("sequences check");
        foreach (BTsequence sequence in sequences)
        {
          if(sequence.evaluate() == BTNODESTATES.FAILURE) {

                Debug.Log("sequence failure moving to other state");
                return true; }
            Debug.Log("no sequence failure ");
        }
       
        return false; // no seqeuence needed execution 

    }

    public override void AITankStart()
    {
        base.AITankStart();












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
