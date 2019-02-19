using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class AStarAlgorithm : MonoBehaviour
{


    /// <summary>
    /// This function finds the shortest path between node Start and node End it was inspired in https://github.com/davecusatis/A-Star-Sharp
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns> List of all the nodes that make the shortest path from start to end (1st=end - last=start) </returns>
    public List<Node> FindPath(Node start, Node end)
    {

        List<Node> Path = new List<Node>();
        List<Node> OpenList = new List<Node>();
        List<Node> ClosedList = new List<Node>();
        List<Node> adjacencies;
        Node current = start;

        // add start node to Open List
        OpenList.Add(start);

        // while we are not out of nodes but still haven't closed "end"
        while (OpenList.Count != 0 && !ClosedList.Contains(end))
        {
            current = OpenList[0]; 
            OpenList.Remove(current); // we will check this node so take it out of the open
            ClosedList.Add(current); // and put it in the closed ones
            adjacencies = current.neighbours; // find neightbour nodes

            foreach (Node n in adjacencies)
            {
                if (!ClosedList.Contains(n))
                {
                    if (!OpenList.Contains(n))
                    {
                        n.Parent = current;
                        n.DistanceToTarget = Mathf.Abs(n.transform.position.x - end.transform.position.x) + Mathf.Abs(n.transform.position.y - end.transform.position.y);
                        n.Cost = 1 + n.Parent.Cost;
                        OpenList.Add(n);
                        OpenList = OpenList.OrderBy(node => node.F).ToList<Node>();
                    }
                }
            }
        }

        // construct path, if end was not closed return null
        if (!ClosedList.Contains(end))
        {
            return null;
        }

        // if all good, return path
        Node temp = ClosedList[ClosedList.IndexOf(current)];
        while (temp != start && temp != null)
        {
            Path.Add(temp);
            temp = temp.Parent;
        }

        return Path;
    }
}
