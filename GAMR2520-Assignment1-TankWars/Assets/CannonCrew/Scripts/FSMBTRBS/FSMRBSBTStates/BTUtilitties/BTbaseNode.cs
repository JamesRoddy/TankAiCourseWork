using UnityEngine;

public abstract class BTbaseNode 
{
    protected BTNODESTATES nodeState;
    

    public BTNODESTATES NodeState
    {

        get { return nodeState; }
        set { nodeState = value; }

    }





    public abstract BTNODESTATES evaluate();





}