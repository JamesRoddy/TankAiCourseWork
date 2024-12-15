using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using static CC_SmartTank;

public class PriorityManager
{
    public enum PRIORITIES
    {
        HEALTH,
        AMMO,
        FUEL,
        NONE,

    }

    List<PriorityHolder> prioritiesList;
    public enum queuePriority
    {
        CRITICAL, MAJOR, MINOR, SAFE
    }

    

    private Dictionary<queuePriority, List<PRIORITIES>> priorityQueues;
    public PriorityManager(List<PriorityHolder> priorities)
    {
        prioritiesList = priorities;
        int prioritiesCount = prioritiesList.Count;
        priorityQueues = new Dictionary<queuePriority, List<PRIORITIES>>
        {
            {queuePriority.CRITICAL,new List<PRIORITIES>(prioritiesCount)},
            {queuePriority.MAJOR,new List<PRIORITIES>(prioritiesCount)},
            {queuePriority.MINOR,new List<PRIORITIES>(prioritiesCount)},
            {queuePriority.SAFE,new List<PRIORITIES>(prioritiesCount)},
           
        };

        for (int i = 0; i<prioritiesList.Count; i++)
        {

            priorityQueues[prioritiesList[i].CurrentClassification].Add(prioritiesList[i].Name);
           

        }
    }



    public PRIORITIES getResources(List<queuePriority> priorities,List<PRIORITIES> resources)
    {

        int maxResources = resources.Count;
        int resourceCount = 0;
        foreach (queuePriority priority in priorities) {

            resourceCount = resourceCount % maxResources;
           
            if (priorityQueues[priority].Contains(resources[resourceCount])) { 
              
                 return resources[resourceCount];
            }

            resourceCount++;
        }

        return PRIORITIES.NONE;


    }


    public   bool hasItems (queuePriority resource)
    {
        return priorityQueues[resource].Count > 0 ;

    }
    public void Update()
    {
        foreach (PriorityHolder priority in prioritiesList)
        {

            if ((priority.CurrentClassification != queuePriority.SAFE &&  priority.checkSafe() )) /// if resource prriority becomes safe and isnt safe already 
            { 
                 Debug.Log(" resource: " + priority.Name + " now safe :prev classification: " + priority.PreviousClassification + " :saftey threshHold: " + priority.SafetyThreshHold + " :currentThreshHold: " + priority.CurrentValue);

                 priorityQueues[priority.PreviousClassification].Remove(priority.Name); // remove it from its current list in the priorityQueues dictionary 
                 priorityQueues[priority.CurrentClassification].Add(priority.Name);// add it to the list under the key SAFE in the dictionary 

               
                 priority.setSafe();
                 

            };

            if (priority.checkForHigherPriorites()) // check if there has been a change in state when it comes to any other priorities other than SAFE 
            {
                
                priorityQueues[priority.PreviousClassification].Remove(priority.Name); // if the state has changed remove the associated resource enum from the associated list that represents the current prirotiy queue the resource is in 
                priorityQueues[priority.CurrentClassification].Add(priority.Name); // add the resource to the queue associated with its updated priority 

               

            }
           





        }

    }

    // check if the resource is in the a queue using the queue enum set to access the assoicated list 

    public List<PRIORITIES> sweepQueues(List<queuePriority> queues) {
        {
            List<PRIORITIES> listPriorities = new List<PRIORITIES>();
            foreach (queuePriority queue in queues)
            {
                foreach (PRIORITIES priority in priorityQueues[queue])
                {
                    listPriorities.Add(priority);

                }
            }


            return listPriorities;


        }

        }
        public bool checkLow(PRIORITIES resource)
        {

            if (priorityQueues[queuePriority.MAJOR].Count > 0 || priorityQueues[queuePriority.CRITICAL].Count > 0 )
            {
               return priorityQueues[queuePriority.MAJOR].Contains(resource) || priorityQueues[queuePriority.CRITICAL].Contains(resource);
            }
            return false;
        }

        public bool checkHigh(PRIORITIES resource) {
            if (priorityQueues[queuePriority.SAFE].Count > 0 || priorityQueues[queuePriority.MINOR].Count > 0)
            {
                return priorityQueues[queuePriority.SAFE].Contains(resource) || priorityQueues[queuePriority.MINOR].Contains(resource);
            }
            return false;
        }
        public bool checkQueue(queuePriority queue, PRIORITIES resource)
        {

      
         return   priorityQueues[queue].Contains(resource);// if the resource is in the priority state being checked


        }
    
        // check if the resource is in the safe queue using the SAFE enum to access the assoicated list 
        public bool isResourceSafe(PRIORITIES resource) {

                return priorityQueues[queuePriority.SAFE].Count > 0 && priorityQueues[queuePriority.SAFE].Contains(resource);


        }


    










}
