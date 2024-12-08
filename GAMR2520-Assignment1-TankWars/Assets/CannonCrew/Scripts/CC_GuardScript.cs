using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PriorityManager;

public class Guard : BaseST
{
    CC_SmartTank Tank;
    GameObject BasePos = new GameObject();
    GameObject OffsetPos = new GameObject();
    Vector3 zOffset = Vector3.zero;
    Vector3 xOffset = Vector3.zero;
    Vector3 Distance = new Vector3(0,0,10);
    float fSpeed = 1f;
    float t;
    float fTime = 8f;
    bool bGen = false;
    bool bStop = false;
    int iCount = 0;

    public Guard(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Update()
    {
        
        if(Vector3.Distance(Tank.transform.position, BasePos.transform.position) > 50f)
        {
            Debug.Log("Moving to base");
           
            Tank.FollowPathToWorldPoint(BasePos, fSpeed, AStar.HeuristicMode.Manhattan);
        }

        else
        {
            bStop = true;

            t += Time.deltaTime;

            if (t < fTime)
            {
               
                Debug.Log("Guarding with timer");
                //if (Vector3.Distance(Tank.transform.position, OffsetPos.transform.position) > 10f)
                //{

                    Tank.GeneratePathToWorldPoint(OffsetPos);
                    Tank.FollowPathToWorldPoint(OffsetPos, fSpeed, AStar.HeuristicMode.Euclidean);

                    Debug.Log("Made it to offset");
                //}

            }

            //else
            //{
                genOffsets();
            //}
            
          
        }
       
        return null;
    }

    private void genOffsets()
    {
        zOffset.z = UnityEngine.Random.Range(1f, 100f);
        xOffset.x = UnityEngine.Random.Range(-100f, 100f);
        OffsetPos.transform.position = BasePos.transform.position + xOffset + zOffset;
        bGen = true;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Guard State");
        BasePos.transform.position = Tank.BasePositionStore;
        genOffsets();
        t = 0f;
        return null;
    }

    public override Type Exit()
    {
        Debug.Log("Exited Guard State");
        zOffset = Vector3.zero;
        xOffset = Vector3.zero;
        t = 0f;
        return null;
    }
}
