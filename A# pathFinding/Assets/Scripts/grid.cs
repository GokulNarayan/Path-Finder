using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grid : MonoBehaviour {


    int penaltyMin = int.MaxValue;
    int penaltyMax = int.MinValue;
    public int obstacleProximityPenalty = 10;
   
    public LayerMask unWalkableMask;

    public bool displayGridGizmos;

    /// <summary>
    /// 2d array of nodes.
    /// </summary>
    Node[,] Grid;

    public TerrainType[] walkableRegions;

    /// <summary>
    /// will contain the layers of all the walkable regions. (Grass and roads)
    /// layers are numbered 1 to 32. each layer is assigned the value 2^n (n being which layer it is.)
    /// </summary>
    LayerMask walkableMask;

    /// <summary>
    /// provides x and z size of the grid.
    /// </summary>
    public Vector2 gridWorldSize;

    /// <summary>
    /// control the size of each node.
    /// </summary>
    public float nodeRadius;

    float nodeDiameter;

    int gridSizeX, gridSizeY;

    Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        //calculates the number of nodes that can be made on one row.
        gridSizeX = Mathf.RoundToInt( gridWorldSize.x / nodeDiameter);
        //calculates the number of nodes that can be made in one column
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y/ nodeDiameter);


        foreach(TerrainType region in walkableRegions)
        {
            walkableMask |= region.layerMask.value;
            walkableRegionsDictionary.Add((int)Mathf.Log(region.layerMask.value, 2), region.terrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        //create the grid 
        Grid = new Node[gridSizeX, gridSizeY];
        //get coordinate of the bottom left corner of the plane. vector3 forward is of form (0,0,1).
        Vector3 worldBottomLeft=transform.position-(Vector3.right*gridWorldSize.x/2)- (Vector3.forward*gridWorldSize.y/2);

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //cycle through each node going from the bottom left.
                //gives the coordinate for the center of the node. eg: x=1, worldbottomleft+ (1,0,0)*(0*diameter + radius) => worldbottomLeft+ (1*radius,0,0).
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                //collision check for each of the nodes
                //true if we dont collide with anything with unwalkable mask
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius,unWalkableMask));

                int movementPenalty = 0;

                //raycast code to find the layer and thus assign a penalty
             
                    Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 100, walkableMask))
                    {
                        walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                    }

                if (!walkable)
                    movementPenalty += obstacleProximityPenalty;
                

                //populate the array made with the information obtained.
                Grid[x, y] = new Node(walkable, worldPoint,x,y,movementPenalty);
            }
        }

        BlurPenalty(3);
    }

    public Node NodeFromWorldPoint(Vector3 worldPos)
    {
        //find how far along the grid the node is.
        float percentX = (worldPos.x + gridWorldSize.x / 2) / (gridWorldSize.x);
        float percentY = (worldPos.z + gridWorldSize.y / 2) / (gridWorldSize.y);
        //make sure the given world pos is within the grid itself.
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        //index of the nodes.
        int x =Mathf.RoundToInt( (gridSizeX - 1) * percentX);
        int y =Mathf.RoundToInt( (gridSizeY - 1) * percentY);

        //return the node using the calculated indices.

        return Grid[x, y];
    }

    public List<Node> getNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();    

        //search for the neighbors using a three by three block 
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(x==0 && y == 0)
                  continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                //check if the neighboring node is actually on the grid.
                if(checkX>=0 && checkX<gridSizeX  && checkY>=0 && checkY < gridSizeY)
                {
                    //add the node to neigbor.
                    neighbors.Add(Grid[checkX, checkY]);
                }
                

            }
        }

        return neighbors;
    }


    void BlurPenalty(int blurSize)
    {
        //A kernel is basically a set of nodes in the grid.
        //it is used to calculate the "blurred" penalty.
        int kernelSize = blurSize * 2 + 1;
        //gives how many squares are on either side of the center square.
        int kernelExtent = (kernelSize - 1) / 2;

        //contains temporary kernels being used.
        int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
        int [,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

        //calculating horizontal values
        for(int y = 0; y < gridSizeY; y++)
        {
            //calculating the left most nodes penalty (horizontal):: eg:  # x x x x x x x x #
            //hash is not part of the grid but will hold the same value as the node next to it.
            //penalty is calculated by adding # and two x's
            for(int x = -kernelExtent; x <= kernelExtent; x++)
            {
                //make sure we use no negative values.
                int sampleX = Mathf.Clamp(x, 0, kernelExtent);
                //add the penalty into the temproray array.
                penaltiesHorizontalPass[0, y] += Grid[sampleX, y].movementPenalty;
            }

            //calculating the rest of the nodes
            for(int x = 1; x < gridSizeX; x++)
            {
                //the following node's new penalty is calculated by using the previous one:
                // eg:w x y z => y's penalty=  x's value + z's value - w's value. (x's value=w+x+y)
                //there should be no overlap of values used.

                //make sure there are no negative values.
                int removeIndex = Mathf.Clamp(x-kernelExtent, 0, gridSizeX);

                //make sure there are no values out of bounds.
                int addIndex = Mathf.Clamp(x + kernelExtent, 0, gridSizeX - 1);

                penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] + Grid[addIndex, y].movementPenalty - Grid[removeIndex, y].movementPenalty;

            }
        }

        //calculating the vertical pass.
        for(int x = 0; x < gridSizeX; x++)
        {
            //calculate the bottom node first.
            for(int y = -kernelExtent; y <= kernelExtent; y++)
            {
                int sampleY = Mathf.Clamp(y, 0, kernelExtent);
                penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                              
            }

            Grid[x, 0].movementPenalty = penaltiesVerticalPass[x, 0]/(kernelSize*kernelSize);


            //calculating the other
            for (int y=1;y<gridSizeY;y++)
            {
                int removeIndex = Mathf.Clamp(y - kernelExtent-1, 0, gridSizeY);
                int addIndex = Mathf.Clamp(y + kernelExtent, 0, gridSizeY - 1);

                penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] + penaltiesHorizontalPass[x, addIndex] - penaltiesHorizontalPass[x, removeIndex];
                //calculate the average of the penalties for each node.
                int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
             
                //reset the node's movement penalty:
                Grid[x, y].movementPenalty = blurredPenalty;

                if (blurredPenalty > penaltyMax)
                    penaltyMax = blurredPenalty;
                if (blurredPenalty < penaltyMin)
                    penaltyMin = blurredPenalty;
            }
        }



    }

    //public List<Node> path;

    void OnDrawGizmos()
    {
        //the Gizmos.draw wire cube method takes in the center and the size 

    
            Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        

        //if (onlyDrawPathGizmo)
        //{
        //    if (path != null)
        //    {
        //        foreach (Node n in path)
        //        {
        //            Gizmos.color = Color.black;
        //            Gizmos.DrawCube(n.nodeWorldPosition, Vector3.one * (nodeDiameter - .1f));
        //        }
        //    }

        //}
        //else
        //{



            if (Grid != null && displayGridGizmos)
            {
                
                //draw each node in scene view
                foreach (Node n in Grid)
                {
                Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax,n.movementPenalty));


                    //decide which color to use if wlkable or not.
                    Gizmos.color = (n.walkable) ?Gizmos.color : Color.red;                           
                    //draw each node.
                    Gizmos.DrawCube(n.nodeWorldPosition, Vector3.one * (nodeDiameter));
                }
            }

           
        }


    }

[System.Serializable]
public class TerrainType
{
    //variable will contain the layer assigned to the terrain
    public LayerMask layerMask;
    //variable will store the penalty assigned to the layr.
    public int terrainPenalty;
}
//}
