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
            Debug.Log(nodeState);

             
            if(nodeState == BTNODESTATES.FORCESUCCES ) // alllow a certain node within the sequence to force a success of the entire sequence used in the retreat state for the action node repsonisble for checking if the enemy tank is there for example 
            {
                Debug.Log("FORCE SUCCESSS");
                return BTNODESTATES.SUCCESS;
                
            }


            if (nodeState == BTNODESTATES.FAILURE) // if one action fails break the sequence
            {  
                break;
            }

        }

        return nodeState;// return current state of sequence





    }



}