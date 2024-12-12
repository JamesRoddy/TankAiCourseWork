/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTActionNode :BTBaseNode
{
    public delegate BTNodeState ActionNodeFunction();

    private ActionNodeFunction btAction;

    public BTActionNode(ActionNodeFunction btAction)
    {
        this.btAction = btAction;
    }

    public override BTNodeState Evaluate()
    {
        switch (btAction())
        {
            case BTNodeState.SUCCESS:
                btNodeStates = BTNodeState.SUCCESS;
                return btNodeStates;
            case BTNodeState.FAILURE:
                btNodeStates = BTNodeState.FAILURE;
                return btNodeStates;
            default:
                btNodeStates = BTNodeState.FAILURE;
                return btNodeStates;

        }
    }
}
*/