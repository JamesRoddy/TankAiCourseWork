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
    Vector3 Distance = new Vector3(0, 0, 15);
    float fSpeed = 1f;
    float t;
    float fTime = 5f;
    bool bGen = false;
    bool bStop = false;
    int iCount = 0;

    public Guard(CC_SmartTank newtank)
    {
        Tank = newtank;
    }

    public override Type Update()
    {

        if (!bStop)
        {
            //Debug.LogError("Going to base");
            bStop = baseDistanceCheck();
        }

        

        t += Time.deltaTime;
        if (bStop && t > fTime)
        {
            Debug.Log("bStop: " + bStop + " t: " + t);
            genOffSets();
            t = 0;
        }

        Debug.Log("Offset Position: " + OffsetPos.transform.position);
      
        Tank.FollowPathToWorldPoint(OffsetPos, fSpeed, AStar.HeuristicMode.Euclidean);
         
        
        if (Tank.enemyTank != null)
        {
            return typeof(Chase);
        }

        return null;
    }

    private void genOffSets()
    {
        Debug.Log("Generate offsets");
        zOffset.z = UnityEngine.Random.Range(20f, 100f);
        xOffset.x = UnityEngine.Random.Range(-100f, 100f);
        OffsetPos.transform.position = BasePos.transform.position + xOffset + zOffset;
    }

    private bool baseDistanceCheck()
    {
        if(Vector3.Distance(BasePos.transform.position, Tank.transform.position) < 20f)
        {
            return true;
        }

        Tank.FollowPathToWorldPoint(BasePos, fSpeed,AStar.HeuristicMode.Manhattan);
        return false;
    }

    public override Type Entry()
    {
        Debug.Log("Entered Guard State");
        BasePos.transform.position = Tank.BasePositionStore - Distance;
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
