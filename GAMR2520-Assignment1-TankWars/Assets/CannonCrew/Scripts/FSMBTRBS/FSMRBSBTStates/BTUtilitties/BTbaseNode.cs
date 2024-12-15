using UnityEngine;

public abstract class BTbaseNode  // bas node clss that allows for the evaulte method to be overriden and redefined through polymorphism 
{
    protected BTNODESTATES nodeState; // descirbe the current state of the node success failure etc 
    

    public BTNODESTATES NodeState
    {

        get { return nodeState; }
      

    }

    public abstract BTNODESTATES evaluate();





}