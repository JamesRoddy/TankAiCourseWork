using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BTbaseNode : MonoBehaviour
{
    protected BTNODESTATES nodeState;


    public BTNODESTATES NodeState
    {
        
        get{ return nodeState; }
        set { nodeState = value; }

    }

   



    public abstract BTNODESTATES evaluate();





}
