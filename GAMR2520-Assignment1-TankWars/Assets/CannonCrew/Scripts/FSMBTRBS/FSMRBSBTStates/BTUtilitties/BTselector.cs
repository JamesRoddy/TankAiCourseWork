using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class BTselector:BTbaseNode
{
    public List<BTbaseNode> actions = new List<BTbaseNode>();
    public  BTselector(List<BTbaseNode> actions)
    {
        Debug.Log("init actions ");
        this.actions = actions;
    }

    public override BTNODESTATES evaluate()
    {

 
        foreach (BTbaseNode action in actions) { 
        

            nodeState = (action.evaluate());
            Debug.Log(nodeState);
            if(nodeState == BTNODESTATES.SUCCESS)
            {

                return  nodeState;
            }
                
        
        }

        return nodeState;


      
        



    }


}
