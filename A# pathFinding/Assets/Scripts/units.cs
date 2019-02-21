using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class units : MonoBehaviour {

    public Transform target;
    public float speed;

    int targetIndex;

    Vector3[] path;

	// Use this for initialization
	void Start () {
        //Request the path.

        pathFindingManager.RequestPath(transform.position, target.position,OnPathReceived);
	}

   public void OnPathReceived(Vector3[] newPath,bool Successful)
    {
        if (Successful)
        {
           // print(newPath.Length);
            path = newPath;
            //start a follow coRoutine.

            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
       // print(path.Length);
        Vector3 currentWayPoint = path[0];

        while (true)
        {
            
            //check if transform has moved to the first way point.
            if (transform.position == currentWayPoint)
            {
                targetIndex++;

                if (targetIndex >= path.Length)
                {
                    yield break;
                }

                currentWayPoint = path[targetIndex];
            }
           
            transform.position = Vector3.MoveTowards(transform.position, currentWayPoint, speed*Time.deltaTime);
            yield return null;
        }

      
         
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for(int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;

                Gizmos.DrawCube(path[i], Vector3.one);
                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
	
}
