using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wrapper for all actions the behaviour tree can perform when in chase state
public class BTActionsChase : MonoBehaviour
{

    CC_smartTankFSMRBSBT Tank;
    float currentSpeed = 1.0f;
    public BTActionsChase(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
    }

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
