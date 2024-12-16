using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wrapper for all actions the behaviour tree can perform when in chase state
public class CC_BTActionsChase : MonoBehaviour
{

    CC_smartTankFSMRBSBT Tank;
    float currentSpeed = 1.0f;
    public CC_BTActionsChase(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
    }

    // through the modulation of the behaviour tree through the use of action nodes with delagtes the chase actions have been simpllfied down to singular method calls to the tanks functionaility
    // athis is also enhanced with all the complexity beigng also managed by the global stats deifned by the rules for exmaple tracking the enemy when we loose sight of them is still determined by 
    // the backwards chaingi used in the RBS system to see if we lost the enemy ewhen we have a conseqeunt state of search allowing for the simplfication of the nodes realted to the chase state while still matining complexity 
    public void moveToBase()
    {

        Tank.FollowPathToWorldPoint(Tank.enemyBase, currentSpeed); 


    }
    public void moveToKnownEnemyPos()
    {

        Tank.FollowPathToWorldPoint(Tank.enemyTank,currentSpeed);


    }

   public  void moveToLastKnownPos()
    {
        Tank.FollowPathToWorldPoint(Tank.LastKnownEPos, currentSpeed);
    }




}
