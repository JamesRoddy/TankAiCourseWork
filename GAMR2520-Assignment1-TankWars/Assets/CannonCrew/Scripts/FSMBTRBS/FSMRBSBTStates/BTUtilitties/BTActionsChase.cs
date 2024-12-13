using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTActionsChase : MonoBehaviour
{

    CC_smartTankFSMRBSBT Tank;
    float currentSpeed = 1.0f;
    public BTActionsChase(CC_smartTankFSMRBSBT tank)
    {
        Tank = tank;
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
