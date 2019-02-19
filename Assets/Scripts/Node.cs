using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{


    public Node Parent;

    public float DistanceToTarget;
    public float Cost;
    public float F
    {
        get
        {
            if (DistanceToTarget != -1 && Cost != -1)
                return DistanceToTarget + Cost;
            else
                return -1;
        }
    }

    public LayerMask wallsAndNodesLayer;
    public int numberOfNeighbours;

    public List<Node> neighbours;

    private void Awake()
    {
        NeighbourFinder();
    }

    /// <summary>
    /// Finds this node's neighbours, where can a ghost go from here
    /// </summary>
    void NeighbourFinder()
    {

        LayerMask notWallsAndNodesLayer = ~(wallsAndNodesLayer);
        RaycastHit2D hit;
        numberOfNeighbours = 0;
        Vector2[] directionVectors = { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        for (int j = 0; j < 4; j++)
        {
            hit = Physics2D.Raycast(transform.position, directionVectors[j], Mathf.Infinity, wallsAndNodesLayer);
            if (hit.collider != null)
            {

                if (hit.collider.tag == "Nodes")
                {
                    neighbours.Add(hit.collider.gameObject.GetComponent<Node>());
                    numberOfNeighbours++;
                }
            };
        }
    }



}
