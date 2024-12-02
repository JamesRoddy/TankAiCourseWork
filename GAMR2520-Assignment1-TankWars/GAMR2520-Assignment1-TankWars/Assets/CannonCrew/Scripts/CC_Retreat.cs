using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retreat : BaseST
{

    private CC_SmartTank Tank;

    public Retreat(CC_SmartTank newtank)
    {
        Tank = newtank;
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
        if(Tank.CheckLowFuel() == false || Tank.CheckLowHealth() == false || Tank.CheckLowAmmo() == false)
        {
            return typeof(SearchState);
        }

        else
        {
            return null;
        }
    }

}
