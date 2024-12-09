using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BTBaseNode 
{
    protected BTNodeState btNodeStates;

    public BTNodeState BNodeStates
    {
        get { return btNodeStates; }
    }

    public abstract BTNodeState Evaluate();
}
