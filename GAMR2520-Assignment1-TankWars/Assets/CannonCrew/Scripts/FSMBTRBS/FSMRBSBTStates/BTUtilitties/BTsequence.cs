using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class BTsequence : BTbaseNode
{

    public List<BTaction> actions;
    public BTsequence(List<BTaction> sequenceOfAtcionsToBeEvaluated)
    {
        Debug.Log("init actions ");
        actions = sequenceOfAtcionsToBeEvaluated;

    }

    public override BTNODESTATES evaluate()
    {
        // the sequeec eis essentially an && gate for the actions that must be successfull in order for the sequence to be complete 
        // therefore if one fails the sequenece is incomplete and therefore broken
 
        foreach (BTaction action in actions)
        { // go through all nodes

            nodeState = action.evaluate();
            Debug.Log("evelauting node state "+nodeState);

            if (action.NodeState == BTNODESTATES.FAILURE) // if one action fails break the sequence
            {
                break;
            }

        }

        return nodeState;// return current state of sequence





    }



}