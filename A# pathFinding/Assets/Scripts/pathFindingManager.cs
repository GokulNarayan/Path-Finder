using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class pathFindingManager : MonoBehaviour {

    /// <summary>
    /// conatins all the pathRequests in a que.
    /// </summary>
    Queue<PathRequest> pathRequestQue = new Queue<PathRequest>();

    /// <summary>
    /// holds the current request being worked on.
    /// </summary>
    PathRequest currentPathRequest;

    /// <summary>
    /// holds an instance of this script itself.needed to access stuff from the static method.
    /// </summary>
    static pathFindingManager instance;

    PathFinding pathfinding;

    bool isProcessingPath;

    //set the instance.
    //get a reference of the pathFinding script.
    void Awake()
    {
        instance = this;
        pathfinding = GetComponent<PathFinding>();
    }


    //Action will hold a method contained in the caller. will be called after map data found.
    public static void RequestPath(Vector3 pathStart,Vector3 pathEnd,Action<Vector3 [],bool> callback)
    {
        //create a new PathRequest using all the data passed to the script.
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        //add this new PathRequest to the que.
        instance.pathRequestQue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    /// <summary>
    /// will  check if a path is being processed. if not, will ask the pathFinding script to process the next one.
    /// </summary>
    void TryProcessNext()
    {

        //check if the queue has some requests in it.
        // check if a path is already being processed.
        if (!isProcessingPath && pathRequestQue.Count>0)
        {
            //set the current request by removing a request from the queue.
            currentPathRequest = pathRequestQue.Dequeue();
            //this current request has to be processed now.
            isProcessingPath = true;
            
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    //will be called by the pathFinding Script after the path has been found.
    //stores variables required for the callback. 
    public void FinishedProcessingPath(Vector3[] path,bool success )
    {
        //invoke the callBack with the informmation.
       
        currentPathRequest.callBack(path, success);
        //the path has now been processed and the next one can be called.
        isProcessingPath = false;
        TryProcessNext();
    }

    //struct that can hold all the  data passed to the method
        struct PathRequest {

        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Vector3[], bool> callBack;

        public PathRequest(Vector3 _start,Vector3 _end,Action<Vector3[],bool> _callBack)
        {
            pathStart = _start;
            pathEnd = _end;
            callBack = _callBack;
        }

    }

	
}
