using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class BTsequence : BTbaseNode
{

    public List<BTbaseNode> actions = new List<BTbaseNode>();
    public BTsequence(List<BTbaseNode> sequenceOfAtcionsToBeEvaluated)
    {
        Debug.Log("init actions ");
        actions = sequenceOfAtcionsToBeEvaluated;

    }

    public override BTNODESTATES evaluate()
    {
        // the sequeec eis essentially an && gate for the actions that must be successfull in order for the sequence to be complete 
        // therefore if one fails the sequenece is incomplete and therefore broken
 
        foreach (BTbaseNode action in actions)
        { // go through all nodes

            nodeState = (action.evaluate());
   

             
            if(nodeState == BTNODESTATES.FORCESUCCES )
            {  

                return nodeState;
            }


            if (nodeState == BTNODESTATES.FAILURE) // if one action fails break the sequence
            {  
                break;
            }

        }

        return nodeState;// return current state of sequence





    }



}