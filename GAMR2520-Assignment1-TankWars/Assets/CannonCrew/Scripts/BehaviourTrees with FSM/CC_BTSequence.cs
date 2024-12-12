/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSequence : BTBaseNode
{
    protected List<BTBaseNode> btNodes = new List<BTBaseNode>();

    public BTSequence(List<BTBaseNode> btNodes)
    {
        this.btNodes = btNodes;
    }

    public override BTNodeState Evaluate()
    {
        bool failed = false;

        foreach (BTBaseNode btNode in btNodes)
        {
            if (failed == true)
            {
                break;
            }

            switch (btNode.Evaluate())
            {
                case BTNodeState.FAILURE:
                    btNodeStates = BTNodeState.FAILURE;
                    failed = true;
                    break;
                case BTNodeState.SUCCESS:
                    btNodeStates = BTNodeState.SUCCESS;
                    continue;
                default:
                    btNodeStates = BTNodeState.FAILURE;
                    failed = true;
                    break;
            }
        }

        return btNodeStates;
    }
}
*/