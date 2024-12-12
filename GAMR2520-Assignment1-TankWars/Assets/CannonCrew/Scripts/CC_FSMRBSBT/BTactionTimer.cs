using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTactionTimer : BTaction
{


    // the action we assigned to the BTaction that will eveulate itself when called 
    private ActionNodeFunction action;

    public delegate bool ProcessTimerCallback(ref float timer); 
    ProcessTimerCallback processTimerCallback;

    public BTactionTimer( ProcessTimerCallback timer, ActionNodeFunction action):base(action)
    {

        processTimerCallback = timer;
    }



    // we  call the eveulate funciton with the associated subscirbed method to the delegate and update ethe state of this node
    // with the vreturned enum of success or failure allowing us to keep track of what parts of the sequence failureed and whihc were a success 
    public override BTNODESTATES evaluate()
    {

        nodeState = action();
        return nodeState;

    }




}
