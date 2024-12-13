using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BTselector:BTbaseNode
{
    public List<BTaction> actions;
    public  BTselector(List<BTaction> actions)
    {
        Debug.Log("init actions ");
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

        return BTNODESTATES.FAILURE;


      
        



    }


}
