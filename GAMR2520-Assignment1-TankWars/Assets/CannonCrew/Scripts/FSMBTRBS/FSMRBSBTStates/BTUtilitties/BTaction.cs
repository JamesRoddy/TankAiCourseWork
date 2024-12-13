using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BTaction : BTbaseNode
{

    // allows a specifc function to be allocated or subscribed to each BTaction which will return a node state 
    public delegate BTNODESTATES ActionNodeFunction();

    // the action we assigned to the BTaction that will eveulate itself when called 
    private ActionNodeFunction action;
    public bool forceSuccess = false;
    public BTNODESTATES forceSuccesOn = BTNODESTATES.SUCCESS;
    public BTaction(ActionNodeFunction action)
    {
        this.action = action;
    }
    public BTaction(ActionNodeFunction action, bool forceSuccess, BTNODESTATES nodeStateToForceSucces)
    {
        this.action = action;
        this.forceSuccess = forceSuccess;
        this.forceSuccesOn = nodeStateToForceSucces;
    }



    // we  call the eveulate funciton with the associated subscirbed method to the delegate and update ethe state of this node
    // with the vreturned enum of success or failure allowing us to keep track of what parts of the sequence failureed and whihc were a success 
    public override BTNODESTATES evaluate()
    {
        
        this.nodeState = action();
    
        return this.nodeState;

    }


}
