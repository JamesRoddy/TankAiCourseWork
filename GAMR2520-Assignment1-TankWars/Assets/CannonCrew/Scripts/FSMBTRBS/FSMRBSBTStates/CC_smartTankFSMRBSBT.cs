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
    public BTaction checkRetreatNeeded;

    public BTaction attackBase;
    public BTaction attackEnemy;
    public BTAttackFunctions tankActionsForAttack;

   public BTsequence attackingBase;
   public BTsequence attackingEnemy;
   public BTsequence retreating;
   public BTselector chasingEnemy;
  


    // timers associated with certain actions

    float attackBaseTimer = 2.0f;



    private  void Awake()
    {

        base.InitiliseStats();
        base.InitiliseRules();
        initStateMachine();

    }

  
    



    private BTNODESTATES ActionCheckBaseRange()
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
    private BTNODESTATES ActionCheckRange()
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
    private BTNODESTATES ActionCheckHighFuel()
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
    private BTNODESTATES ActionCheckHighAmmo()
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

    private BTNODESTATES ActionCheckEnemyBaseVisisble()
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

    private BTNODESTATES ActionCheckEnemyVisisble()
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
    private BTNODESTATES isNotInRangeOfBase()
    {
        if (!stats["enemyBaseWithinRange"])
        {
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }
    }
    private BTNODESTATES ActionCheckCanAttackBase()
    {
        if (stats["canAttackBase"])
        {
            return BTNODESTATES.FAILURE;




        }
        else
        {

            return BTNODESTATES.SUCCESS;
        }


    }

    private BTNODESTATES ChaseTargetNotVisisble()
    {

        if (stats["lostSight"])
        {
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }
    }

    private BTNODESTATES ActionCheckHighHealth()
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

    private BTNODESTATES ActionCheckLowFuel()
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
    private BTNODESTATES ActionCheckLowAmmo()
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

    private BTNODESTATES ActionCheckShouldNotRetreat()
    {
        if (!stats["shouldRetreat"])
        {

            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }


    }


    private BTNODESTATES ActionCheckShouldChase()
    {

        if (stats["shouldChase"])
        {

            return BTNODESTATES.FAILURE;



        }
        else
        {
            return BTNODESTATES.SUCCESS;

        }




    }


    private BTNODESTATES ActionCheckLowHealth()
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


    private BTNODESTATES ActionCanAttackEnemyBase()
    {

        if (stats["canAttackBase"])
        {
            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;


        }

    }


    private BTNODESTATES ActionCanAttackEnemy()
    {

        if (stats["canAttack"])
        {

            return BTNODESTATES.FAILURE;

        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }

    }

    private BTNODESTATES ActionCheckLostSight()
    {

        if (stats["lostSight"])
        {
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }



    }


    private BTNODESTATES ActionCheckShouldRetreat()
    {
        if (stats["shouldRetreat"])
        {
            return BTNODESTATES.FAILURE;
        }
        else
        {
            return BTNODESTATES.SUCCESS;
        }
    }

    private BTNODESTATES ActionmCheckCriticalAmmo()
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
            
            { typeof(BTFSMRBSAttack),new BTFSMRBSAttack(this)},
            

        };

        GetComponent<CC_FSM>().setStates(states);




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



    private void initBt()
    {

        tankActionsForAttack = new BTAttackFunctions(this);

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
        checkShouldAttack = new BTaction(ActionCanAttackEnemy);
        checkShouldRetreat = new BTaction(ActionCheckShouldNotRetreat);
        checkShouldAttackBase = new BTaction(ActionCanAttackEnemyBase);
        checkEnemyBaseVisible = new BTaction(ActionCheckEnemyVisisble);
        checkEnemyVisible = new BTaction(ActionCheckEnemyBaseVisisble);
        checkWihtinRangeOfBase = new BTaction(ActionCheckBaseRange);
        checkRetreatNeeded = new BTaction(ActionCheckShouldNotRetreat);

        attackingEnemy = new BTsequence(new List<BTaction> { checkRetreatNeeded, checkShouldAttack, withinRangeCheck });
        attackingBase = new BTsequence(new List<BTaction> { checkShouldAttackBase, checkWihtinRangeOfBase });
        chasingEnemy = new BTselector(new List<BTaction> {  checkLostTarget, checkShouldChase });
        retreating = new BTsequence(new List<BTaction> { checkShouldRetreat });




    }


    public bool evaluateSequences(List<BTsequence> sequences)
    {
        foreach (BTsequence sequence in sequences)
        {

            foreach (BTaction action in sequence.actions)
            {
                if (action.evaluate() == BTNODESTATES.SUCCESS)
                {
                    return true;
                }
            }

        }

        return false;

    }

    public override void AITankStart()
    {
        base.AITankStart();

        initBt();










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
