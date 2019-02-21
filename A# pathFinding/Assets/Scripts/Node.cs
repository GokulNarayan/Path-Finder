using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node:IHeapItem<Node> {

    /// <summary>
    /// tells the node if it is walkable or not
    /// </summary>
    public bool walkable;
    /// <summary>
    /// essential to know which point in the world the node represents.
    /// </summary>
    public Vector3 nodeWorldPosition;

    /// <summary>
    /// rays will be shot onto the ground and will check what is beiung hit.
    /// it will also check the movement penalty which will make the object stick to the preferred path.
    /// </summary>
    public int movementPenalty;
    
    /// <summary>
    /// gCost is the distance between the node and the start node.
    /// </summary>
    public int gCost;
    /// <summary>
    /// fCost is the distance between the node and end node.
    /// </summary>
    public int hCost;

    /// <summary>
    /// never need to assign to fcost. always calculated.
    /// </summary>
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    /// <summary>
    /// holds parent of this class.
    /// </summary>
    public Node parent;

    /// <summary>
    /// holds the nodes index in the 2d array.
    /// </summary>
    public int gridX;
    public int gridY;


    int heapIndex;

    //constructor
    public Node(bool _walkable, Vector3 _worldPos,int _gridX,int _gridY,int _penalty)
    {
        walkable = _walkable;
        nodeWorldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        movementPenalty = _penalty;
    }

    //implement the things in the interface.

    public int HeapIndex {

        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        //if fcost of this node is greater,returns 1, else -1.
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        //if the f cost is the same, compare the hcost
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        //the node system used is reversed. 
        // if a node has a "higher" priority, it has a lesser f cost.
        return -compare;
    }



}
