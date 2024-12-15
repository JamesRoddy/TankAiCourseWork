using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;
using static BTaction;
using UnityEngine.Video;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
public class CC_smartTankFSMRBSBT : CC_SmartTankRBS
{


    
    // all action nodes of the tree 
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
    public BTaction checkShouldChaseBase;
    public BTaction checkShouldChase;
    public BTaction checkLostTarget;
    public BTaction checkShouldAttack;
    public BTaction checkShouldRetreat;
    public BTaction checkShouldAttackBase;
    public BTaction checkEnemyBaseVisible;
    public BTaction checkLostSightInSearch;
    public BTaction checkWihtinRangeOfBase;
    public BTaction moveToConsumable;
    public BTaction checkForConsumable;
    public BTaction findPriorityResource;
  
    
    

   BTaction checkEnemyPos;
   BTaction lookForEnemy;
   BTaction getSafeSpot;
   BTaction moveToSafeSpot;
    // selectors and sequences for the tree allowing for multiple actions to be chained and evaulted togther 
   public BTselector attackingBase; // selector for attacking enemy base and checking if we are in range 
   public BTselector attackingEnemy;// selector for attacking enemy and if we are in range if one succeed then we will nnot attack
   public BTsequence retreating; // sequnce fo retreating inlcuidg looking behind moving to sfatey spot and getting sfatey spot 
   public List<BTsequence> sequencesFromSearch ; // define all sequnces that if met will rantions us out of search
   public List<BTselector> selectorsForAttack; // wrapp all the selectors for attack into one list so they can be looped through and checked at once
   public BTsequence chasingEnemy; // seqeunce for checking if we lost sight of the enemy during chase so we chase the last knwon postion or if we should chase the enemy weh they are visisble
   public BTaction chasingBases; // sequence for getting close to a base and shooting it to ensure we dont miss 
 


    public BTsequence findingConsumables; // sequence for findnig consumables and evaluating their prirority

   public BTselector switchFromSearch;

   private BTAttackActions tankActionsForAttack;
   private BTActionsSearch tankActionsForSearch;
   private BTActionsChase tankActionsForChase;
   private BTactionRetreat tankActionsForRetreat; 

   public BTsequence checkHighHealthAndBaseVisbible;
    public BTsequence checkLowHealthAndEnemyVisible;

    public BTsequence checkSearchSwitch;
   private List<queuePriority> priorityOrder =  new List<queuePriority> { queuePriority.MAJOR, queuePriority.CRITICAL };
   private List<PRIORITIES> resourcePriorityOrder = new List<PRIORITIES> { PRIORITIES.HEALTH, PRIORITIES.FUEL, PRIORITIES.AMMO };
   public PRIORITIES currentPriorityResource;
   public BTsequence checkHighHealthAndTargetVisbible;

    List<Rule> rulesForFSMRBSBT;// initilaise rules for bt


    
   private CC_BTFSMRBChaseState chaseDebugs;
   private CC_smartTankFSMRBSBT debugTanks;
   private BTFSMRBSAttack attackDebugs;
   private CC_BTFSMRBSRetreatState retreatDebugs; 
   private CC_BTFSMRBSSearchState searchDebugs;
    




    private  void Awake()
    {

        debugTanks = new CC_smartTankFSMRBSBT();
        chaseDebugs = new CC_BTFSMRBChaseState(debugTanks);
        attackDebugs = new BTFSMRBSAttack(debugTanks);
        retreatDebugs = new CC_BTFSMRBSRetreatState(debugTanks);
        searchDebugs = new CC_BTFSMRBSSearchState(debugTanks);
       
        

       
    }



    // deifne various action node functions that corrleate to particualr nodes in the tree using the stats defined by the rule based system to detemine the state of the action along with if the tank is performing a particualr action such as waiting 

    public BTNODESTATES checkShouldSearch() // allows the search satte to be contious while also allowing for sequences to be evelauted that may cause a switch from search state 
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

    public BTNODESTATES getConsumable()
    {

        if (tankActionsForSearch.moveToConsumable()) // if we have reached the consumable we saw initially 
        {
            Debug.Log("has moved to consumable in front ");

            tankActionsForSearch.hasFoundResource = false; // we collected a reosurce so we no longer see one
            if (stats["hasReachedPriorityMax"]) /// if we are taking this consumable beacuse we failed to find the priority resource a number of times(3)
            {
                Debug.Log("has moved to conusmable in front but still has prirority");

                tankActionsForSearch.resetPriorityResourceNotFoundCounter(); // reset counter before retruing success 
                
                return BTNODESTATES.FAILURE; // we couldnt find priorty resource so return failure to reset sequence
            }
            return BTNODESTATES.SUCCESS;
        }
        else
            Debug.Log("has not reached consumable in front ");
        {
            return BTNODESTATES.FAILURE;
        }


    }
    public BTNODESTATES findPriorityConsuamble()
    {

        if (stats["hasReachedPriorityMax"])                                                     // if weve not found the priority resource a scertain number of times then we will take the reource weve found anyway and succed this part of the sequence to do so
        {
            Debug.Log("hit  max for  not finding priroity resource ");

            return BTNODESTATES.SUCCESS;
        }

        if (stats["hasPriorityResource"]  )                                             // if we have a priroity resource that needs tending to(major or critical)
        {

            if (tankActionsForSearch.findPriorityResource(currentPriorityResource))                  // if we have reached the position
            {
                Debug.Log("has  completed finding priroity resource ");

                return BTNODESTATES.SUCCESS;                                            // set the action node's state to success 
            }
            Debug.Log("has  not completed finding priority resource ");


            return BTNODESTATES.FAILURE;
        }
       
        else
        {
            Debug.Log("has no priroity resource moving to collect conusmable in front");
            return BTNODESTATES.FORCESUCCES;
        }



    } 

    public BTNODESTATES ActionSeeConsumable ()
    {
        if (stats["enemySeen"]) // if we see the enemy we need to eveaulte what to do so we drop out of the sequence to find consumables
        {
            return BTNODESTATES.FORCESUCCES;
        }

        if (stats["hasSeenConsumable"] || tankActionsForSearch.hasFoundResource || stats["hasPriorityResource"]  ) // if we see a consumable or we known weve  found one
        {   
            tankActionsForSearch.hasFoundResource  = true;
            if(currentPriorityResource != PRIORITIES.NONE && !seeResource(currentPriorityResource)) // if we have a current resource of priority and the one we are looking at doesnt mact
            {
                Debug.Log("has found consumable we was not of priroity so increment counter ");
                tankActionsForSearch.incrementPriorityResourceNotFoundCounter(); // count how many times we dont find our resource 

            }
            Debug.Log("go to finding resource ");

            return BTNODESTATES.SUCCESS; // continue on to the next part of the sequence
            
        }
        else
        {
            Debug.Log("has found consumable or does not see consumable force success r ");

            return BTNODESTATES.FORCESUCCES;// otherwise we dont need to execute the sequnce so force success 
        }



    }
   


    public BTNODESTATES checkPosRetreat()
    {

        if (!tankActionsForRetreat.hasFinishedRun() &&!tankActionsForRetreat.CheckPositionReference()) // if we are still runnning and dont need a position refernce we succed the wait automatically
        {
            Debug.Log("no need to wait ");
            return BTNODESTATES.SUCCESS;
        }

        if (!tankActionsForRetreat.checkEtankPos()) // certain node states are based on the success of actions that need to be performed by the tank istelf 
        {

            Debug.Log("wait not complete ");
            return BTNODESTATES.FAILURE;
        }
         if (stats["enemySeen"] == false) // if we ddint see the enemy tank that means the retreat was successful and we are safe so the sequence is 'forced' inot success through the force success state for nodes 
          {
              Debug.Log("found  no tank in retreat");
              tankActionsForRetreat.resetTimers();

               return BTNODESTATES.FORCESUCCES; // consider the whole sequence complete 
          }
          else
          {
               
                Debug.Log("found tank continue retreat  sequence");
                return BTNODESTATES.SUCCESS; // otherwise if we saw the enemy tank we need to continue the sequence regardless 
          }
            // for exmaple here the checkEpos method inside the retreat actions wrapper will return true if the wait time for checkin the position is reached or if the enemy tank was seen 
      
       
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
    public BTNODESTATES isInSearchAndLostSight()
    {
        if (stats["lostSight"] == true)
        {
            Debug.Log("lost sight in search");

            return BTNODESTATES.SUCCESS;
        }
        else
        {
            return BTNODESTATES.FAILURE;
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

          
            return BTNODESTATES.SUCCESS;
        }
        else
        {

            return BTNODESTATES.FAILURE;
        }

    }

    public BTNODESTATES ActionCheckEnemyVisisble()
    {

        if (stats["enemySeen"])
        {

            

            return BTNODESTATES.SUCCESS;
        }
        else
        {
            return BTNODESTATES.FAILURE;


        }

    }

    
    public BTNODESTATES ActionCheckCanAttackBase()
    {
        
        if ( BtStatMultiQuery("canAttackBase") || stats["btBaseDead"] == false)
        {

            
            return BTNODESTATES.FAILURE;

        }
        else 
        {
            tankActionsForAttack.ResetTimer();

            return BTNODESTATES.SUCCESS;
        }
        

    }

    public BTNODESTATES ChaseTargetNotVisisble()
    {
        /// this is still evaluated by the backwards chaining in the 
        /// RBSFSM system to identify whether or not we lost the enemy when switching to the search state 
        if (stats["lostSight"]) 
        {
            Debug.Log("chasing enemt last known pso btfsmrbs");
            tankActionsForChase.moveToLastKnownPos();// use the moveToBase function defined in the BTactionsFromChase wrapper class  to have the tank chase the enemy based on their last known position allowing us to catch up when ew loose sight 
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
           

            return BTNODESTATES.SUCCESS;
        }
        else
        {
            return BTNODESTATES.FAILURE;
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

    // evalute chase
    public BTNODESTATES chaseBase()
    {
        if (BtStatMultiQuery("chaseBase"))
        {
        
            tankActionsForChase.moveToBase(); // use the moveToBase function defined in the BTactionsFromChase wrapper class  to have the tank get in range of the base before firing to secure the shot 
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }

    }
    public BTNODESTATES ActionCheckShouldChase()
    {

        if (BtStatMultiQuery("shouldChase")) // the BTStatMultiQuery allows us to still make us of the multi stat containers defined by the rule based system but ommit certain thigs such as whetehr or not the current state beigng run is true or false to avoid clashes 
        {

            Debug.Log("chasing enemt btfsmrbs");
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
       

            return BTNODESTATES.SUCCESS;
        }
        else
        {
            return BTNODESTATES.FAILURE;
        }




    }


    
    // check if we can atttack the enemy by querying the canAttack stat which complies
    // multiple stats into a singular check to evaluate the situation 
    public BTNODESTATES ActionCanAttackEnemy()
    {
        
        if (BtStatMultiQuery("canAttack"))
        {
            
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
            Debug.Log("lost sight");
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

            Debug.Log("should retreat");
            return BTNODESTATES.SUCCESS;
        }
        else
        {
            return BTNODESTATES.FAILURE;
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

    void initBTSpecifcStats()
    {
        stats.Add("btBaseDead",false);
        stats.Add("hasSeenConsumable",false);
        stats.Add("hasPriorityResource", false);
        stats.Add("hasReachedPriorityMax", false);
    }

    void updateBTSpecifcStats()
    {
        stats["btBaseDead"] = tankActionsForAttack.baseTimerIncrement >= tankActionsForAttack.baseDeadTimer;
        stats["hasSeenConsumable"] = consumablesFound.Count > 0;
        stats["hasPriorityResource"] = currentPriorityResource != PRIORITIES.NONE;
        stats["hasReachedPriorityMax"] = tankActionsForSearch.priorityResourceNoHitMax <= tankActionsForSearch.priorityResourceNotHitCounter;

    }
    public bool evaluateSelectors(List<BTselector> selectors) // util function to evaluate multiple sequences at once
    {
     
        BTNODESTATES result = BTNODESTATES.FAILURE;
        foreach (BTselector selector in selectors)
        {
            

            result = selector.evaluate();
             if(result == BTNODESTATES.SUCCESS)
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
          if(sequence.evaluate() == BTNODESTATES.SUCCESS) {

                return true; 
          }
        }
       
        return false; // no seqeuence needed execution 

    }

    public override void AITankStart()
    {
        base.AITankStart(); // call base start(FSMRBS)

        InitiliseStats(); /// intiilai stats of the original FSMRBS
        initBTSpecifcStats(); // allow for intilaisation of stats unquie to the behvaiour tree not related to the base rule based system 
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
      
        // intilaise wrapper for actions associated with various sequence and selctors or action nodes such as attacking a base chaing  a base to get in range so we dont miss a shot, retreating, looking behind and waiting, etc 
        tankActionsForAttack = new BTAttackActions(this);
        tankActionsForSearch = new BTActionsSearch(this);
        tankActionsForChase = new BTActionsChase(this);
        tankActionsForRetreat = new BTactionRetreat(this);


        // intilaing the action nodes of the tree with their associated function to be subscribed to the delegate within the action node object 

        checkEnemyPos = new BTaction(checkPosRetreat);
        getSafeSpot = new BTaction(getSafteySpot);
        moveToSafeSpot = new BTaction(moveToSaftey);

        checkForConsumable = new BTaction(ActionSeeConsumable);
        findPriorityResource = new BTaction(findPriorityConsuamble);
        moveToConsumable = new BTaction(getConsumable);


        targetVisisbleCheck = new BTaction(ActionCheckEnemyVisisble);
        withinRangeCheck = new BTaction(ActionCheckRange);
        checkHighHealth = new BTaction(ActionCheckHighHealth);
        checkLowHealth = new BTaction(ActionCheckLowHealth);
        checkHighAmmo = new BTaction(ActionCheckHighAmmo);
        checkLowAmmo = new BTaction(ActionCheckLowAmmo);
        checkAmmoCritical = new BTaction(ActionmCheckCriticalAmmo);
        checkLostSightInSearch = new BTaction(isInSearchAndLostSight);


    
        checkHighFuel = new BTaction(ActionCheckHighFuel);
        checkLowFuel = new BTaction(ActionCheckLowFuel);

        checkShouldChase = new BTaction(ActionCheckShouldChase);
        checkLostTarget = new BTaction(ActionCheckLostSight);
        checkShouldChaseBase = new BTaction(chaseBase);
        checkSearch = new BTaction(checkShouldSearch);
        checkShouldAttack = new BTaction(ActionCanAttackEnemy);
        checkShouldRetreat = new BTaction(ActionCheckShouldRetreat);
        checkShouldAttackBase = new BTaction(ActionCheckCanAttackBase);
        checkEnemyBaseVisible = new BTaction(ActionCheckEnemyBaseVisisble);
        checkWihtinRangeOfBase = new BTaction(ActionCheckBaseRange);
        
        switchFromSearch = new BTselector(new List<BTbaseNode> {  targetVisisbleCheck, checkLostSightInSearch,checkSearch });
        attackingEnemy = new BTselector(new List<BTbaseNode> {  checkShouldAttack, withinRangeCheck, });
        attackingBase = new BTselector(new List<BTbaseNode> { checkShouldAttackBase, checkWihtinRangeOfBase });
        chasingEnemy = new BTsequence(new List<BTbaseNode> { checkLostTarget, checkShouldChase });
        
        retreating = new BTsequence(new List<BTbaseNode> { checkEnemyPos, getSafeSpot, moveToSafeSpot }); // sequence for retreating one action node that being the node for checking the enemy is there hasa force success as it is not the ned of the sequnce but is a potential break point if we dont see the enemy after looking behind 
        findingConsumables = new BTsequence(new List<BTbaseNode> { checkForConsumable, findPriorityResource, moveToConsumable });

        // health governs intial decisions as to whether or not we engage
        // with the enemy after that we will then check reosurces such as fuel and ammo 
        // which are eveualted and updated by the rules and stats 
        checkHighHealthAndTargetVisbible = new BTsequence(new List<BTbaseNode> { checkHighHealth, targetVisisbleCheck }); 
        checkHighHealthAndBaseVisbible = new BTsequence(new List<BTbaseNode> { checkHighHealth, checkEnemyBaseVisible });        

        checkLowHealthAndEnemyVisible = new BTsequence(new List<BTbaseNode> { checkLowHealth, targetVisisbleCheck}); // seqeunces wrapped in lists so multiple can be evalulated at once through the evelaute sequences utility method 
        checkSearchSwitch = new BTsequence(new List<BTbaseNode> { checkSearch }); 
        sequencesFromSearch = new List<BTsequence> { checkHighHealthAndTargetVisbible,checkHighHealthAndBaseVisbible,checkLowHealthAndEnemyVisible,checkSearchSwitch }; // allows us to evelaute multiple seuqences at once to se if we should go into other states from search for exmaple
        
        



        initStateMachine(); // inti state machine for FSMRBSBT








    }

    public override void AITankUpdate()
    {
        base.AITankUpdate();
        currentPriorityResource = priorityManager.getResources(priorityOrder, resourcePriorityOrder);

        updateBTSpecifcStats();






    }

    public override void AIOnCollisionEnter(Collision collision)
    {
        base.AIOnCollisionEnter(collision);




    }



















}
