/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTSelector : BTBaseNode
{
    protected List<BTBaseNode> btNodes = new List<BTBaseNode>();

    public BTSelector(List<BTBaseNode> btNodes)
    {
        this.btNodes = btNodes;
    }

    public override BTNodeState Evaluate()
    {
        foreach (BTBaseNode btNode in btNodes)
        {
            switch (btNode.Evaluate())
            {
                case BTNodeState.FAILURE:
                    continue;
                case BTNodeState.SUCCESS:
                    btNodeStates = BTNodeState.SUCCESS;
                    return btNodeStates;
                default:
                    continue;
            }
        }

        btNodeStates = BTNodeState.FAILURE;
        return btNodeStates;
    }
}
*/