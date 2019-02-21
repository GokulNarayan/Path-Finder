using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;

public class PathFinding : MonoBehaviour {


    //public Transform Seeker;
    //public Transform target;


    pathFindingManager requestManager;

    /// <summary>
    /// reference of the grid script attached on this object.
    /// </summary>
    grid Grid;

    void Awake()
    {
        Grid = GetComponent<grid>();
        requestManager = GetComponent<pathFindingManager>();
    }


    //void Update()
    //{
    //    if(Input.GetButtonDown("Jump"))
    //    FindPath(Seeker.position, target.position);
    //}

    /// <summary>
    /// This method is called from the pathFinding manager.
    /// </summary>
    /// <param name="StartPosition"></param>
    /// <param name="EndPosition"></param>
    public void StartFindPath(Vector3 StartPosition, Vector3 EndPosition)
    {
        //initiates the find path method.
        StartCoroutine(FindPath(StartPosition, EndPosition));

    }



    /// <summary>
    /// this method will create the path that the object has to take.
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>

    IEnumerator FindPath(Vector3 startPosition, Vector3 endPosition)
    {
        Stopwatch sw = new Stopwatch();

        sw.Start();

        Vector3[] waypoints = new Vector3[0];
        bool PathSuccess = false;

        //converts the vector3 position to the node on the map.
        Node startNode = Grid.NodeFromWorldPoint(startPosition);
        Node endNode = Grid.NodeFromWorldPoint(endPosition);

        //list for containing all the open nodes.
        Heap<Node> openSet = new Heap<Node>(Grid.MaxSize);

        //HashSEt holds a set of objects making it easy to check if a certain object is already in it.
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            //find node in open set with lowest f cost
            Node currentNode =openSet.RemoveFirst();

           

            closedSet.Add(currentNode);

            //if the node found with lowest fcost is the target, return.
            if (currentNode == endNode)
            {
                //if path is found, stop the stopWatch

                sw.Stop();
                print("path found " + sw.ElapsedMilliseconds + "ms");

                //if path is found, it is successful.
                PathSuccess = true;
                
                //if pathFound, exit out of the loop.
                break;
            }

            foreach (Node neighbor in Grid.getNeighbors(currentNode))
            {
                //if the neighbor is not traversable or is already in the closed set, skip
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    continue;
                }
                //add the movement penalty to the cost to the neighbor too.
                int newMovementCostToNeighbor = currentNode.gCost + getDistance(currentNode, neighbor)+neighbor.movementPenalty;

                // if the distance between the current node and the neighbor is less than the gcost or neighbor is not in the open list
                if(newMovementCostToNeighbor<neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = getDistance(neighbor, endNode);

                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else
                    {
                        openSet.UpdateItem(neighbor);
                    }
                }

                
            }
                      
        }

        //makes it wait for one frame.
        yield return null;
        //after  finding the path:
        //if a path has been found, store each node of the path into the array.
        if (PathSuccess)
        {
           
            waypoints= ReTracePath(startNode, endNode);
        }
        //the reqested data has been obtained.Process is finished
        requestManager.FinishedProcessingPath(waypoints, PathSuccess);

    }

   

    Vector3[] ReTracePath(Node StartNode,Node EndNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = EndNode;
        

        //until the current node is the start node 
        while (currentNode!=StartNode)
        {
            path.Add(currentNode);
            //set current node as the parent .
            currentNode = currentNode.parent;
        }

        

       //returns a simplified version of the path.
        Vector3[] waypoints = simplifyPath(path);

        //path goes from end to start => reverse
        Array.Reverse(waypoints);

        return waypoints;   



       // Grid.path = path; 
    }

    /// <summary>
    /// only stores the next point if a change in direction is encountered..
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    Vector3[] simplifyPath(List<Node> Path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 oldDirection=Vector2.zero;

        for(int i = 1; i < Path.Count; i++)
        {
            //get the direction between the two points.
            Vector2 newDirection = new Vector2(Path[i - 1].gridX - Path[i].gridX, Path[i - 1].gridY - Path[i].gridY);

            //check if the old direction and new direction are the same.
            if (oldDirection != newDirection)
            {
                //add the new point to the list of wayPoints.
                waypoints.Add(Path[i].nodeWorldPosition);
                //set the old direction.
                oldDirection = newDirection;
            }

        }

        return waypoints.ToArray(); 

    }

    int getDistance(Node A, Node B)
    {
        int distanceX = Mathf.Abs(A.gridX - B.gridX);
        int distanceY = Mathf.Abs(A.gridY - B.gridY);

        int diagonalMoves = distanceX > distanceY ? distanceY : distanceX;
        int horizontalMoves = distanceX > distanceY ? (distanceX - distanceY) : (distanceY - distanceX);

        return 14 * diagonalMoves + 10 * (horizontalMoves);
    }

}
