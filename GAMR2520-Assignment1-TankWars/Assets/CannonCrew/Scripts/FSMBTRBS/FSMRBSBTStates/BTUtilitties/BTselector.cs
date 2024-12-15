using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BTselector:BTbaseNode //s electors act as an or gate for action nodes and willl evelaute until at least when is a success
{
    public List<BTbaseNode> actions = new List<BTbaseNode>();
    public  BTselector(List<BTbaseNode> actions)
    {
        Debug.Log("init actions ");
        this.actions = actions;
    }

    public override BTNODESTATES evaluate()
    {

 
        foreach (BTbaseNode action in actions) {  // go through all actions required
        

            nodeState = (action.evaluate()); // get state of action node 
            Debug.Log(nodeState);
            if(nodeState == BTNODESTATES.SUCCESS)// on success delcare the selection as a success 
            {

                return  nodeState;
            }
                
        
        }

        return nodeState;
        // return failure if no success(essentially an or opertaion based on whether or not actions succed or fail)

      
        



    }


}
