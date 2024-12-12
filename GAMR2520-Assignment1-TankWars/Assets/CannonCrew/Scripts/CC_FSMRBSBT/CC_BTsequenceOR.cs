using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSelector : BTbaseNode
{

    List<BTaction> actions;

    public BTSelector(List<BTaction> actions)
    {

        this.actions = actions;


    }



    public override BTNODESTATES evaluate()
    {
        foreach (BTaction action in actions) { 
        
          nodeState = action.evaluate();
          if(nodeState == BTNODESTATES.SUCCESS)
          {
                return BTNODESTATES.SUCCESS;
          }
           
        
        
        }
        return nodeState;


    }



}
