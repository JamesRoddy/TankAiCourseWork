using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// all possible node states for a node of the tree 
public enum BTNODESTATES
{
    SUCCESS, // used to control if things such as sequences are continued or whihc node within a slector is chosen 
    FAILURE, // if failure the action is condisered to be incomplete and if the node is witthin a sequence it will need to restart and if the node is in a selctor it will not be chosen
    FORCESUCCES, // allows a node within a sequnce to force the success of the entire sequnce allowing their to be certain break points defined wihtin the sequence meaning the transitions to other behaviour is possible during the ssequence through the break points where a nodes state forces the success of the sequence 
    REPEAT, // allows for a node to be marked as a reapte node meaning if a node is hit and returns srepeat during a sequence for example it will force the sequnce to restart 
}
