using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CC_SmartTank;

public class SearchState : BaseST
{

    private CC_SmartTank tank;

    
    public SearchState(CC_SmartTank newTank )
    {
        tank = newTank;

    }
    public override Type Entry()
    {
        return null;
    }
    public override Type Exit()
    {
        return null;
    }
    public override Type Update()
    {

        if (tank.hasLowResource())
        {
            PRIORITIES currentLowest = tank.GetCurrentLowestResource(); 

            if(currentLowest == PRIORITIES.HEALTH)
            {

            }



        }
        


        return null;
    }





}
