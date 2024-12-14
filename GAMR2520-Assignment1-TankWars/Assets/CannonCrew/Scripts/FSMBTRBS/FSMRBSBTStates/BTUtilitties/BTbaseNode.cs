using UnityEngine;

public abstract class BTbaseNode 
{
    protected BTNODESTATES nodeState;
    

    public BTNODESTATES NodeState
    {

        get { return nodeState; }
      

    }





    public abstract BTNODESTATES evaluate();





}