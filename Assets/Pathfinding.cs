using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



public class PathfindingNode
{
    public Vector3 NodePoint;
    public PathfindingNode ParentNode;
    public int GCost { get; set; }
    public int HCost { get; set; }
    public int FCost => GCost + HCost;
        
    public List<PathfindingNode> Neighbours { get; set; }
        
    public bool Walkable { get; set; }

    public Vector3 GetNodePosition()
    {
        return this.NodePoint; 
    }

    public PathfindingNode(Vector3 Point)
    {
        this.NodePoint = Point;
        this.ParentNode = null;
        this.GCost = 0;
        this.HCost = 0;
    }
}


public static class Pathfinding
{

    /// <summary>
    ///     Finds the shortest path between two node points using A* algorithm
    /// </summary>
    /// <param name="StartingNode"></param>
    /// <param name="EndNodePoint"></param>
    /// <returns></returns>
    public static List<PathfindingNode> FindPath(PathfindingNode StartingNode, PathfindingNode EndNodePoint)
    {
        List<PathfindingNode> openSetNodePoints = new List<PathfindingNode>();
        HashSet<PathfindingNode> closedNodePoints = new HashSet<PathfindingNode>();
        openSetNodePoints.Add(StartingNode);
        while (openSetNodePoints.Count > 0)
        {
            PathfindingNode currentNode = openSetNodePoints[0];
            for (int i = 1; i < openSetNodePoints.Count; i++)
            {
                if (openSetNodePoints[i].FCost < currentNode.FCost || openSetNodePoints[i].FCost == currentNode.FCost && openSetNodePoints[i].HCost < currentNode.HCost)
                {
                    currentNode = openSetNodePoints[i];
                }
            }
            openSetNodePoints.Remove(currentNode);
            closedNodePoints.Add(currentNode);
            if (currentNode == EndNodePoint)
            {
                return RetracePath(StartingNode, EndNodePoint);
            }
            foreach (PathfindingNode neighbour in currentNode.Neighbours)
            {
                if (!neighbour.Walkable || closedNodePoints.Contains(neighbour))
                {
                    continue;
                }
                int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.GCost || !openSetNodePoints.Contains(neighbour))
                {
                    neighbour.GCost = newMovementCostToNeighbour;
                    neighbour.HCost = GetDistance(neighbour, EndNodePoint);
                    neighbour.ParentNode = currentNode;
                    if (!openSetNodePoints.Contains(neighbour))
                    {
                        openSetNodePoints.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    ///     Gets the distance between two nodes
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns></returns>
    private static int GetDistance(PathfindingNode nodeA, PathfindingNode nodeB)
    {
        int dstX = Mathf.Abs((int)nodeA.NodePoint.x - (int)nodeB.NodePoint.x);
        int dstY = Mathf.Abs((int)nodeA.NodePoint.y - (int)nodeB.NodePoint.y);
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }
    
    /// <summary>
    ///     Retraces the path from the end node to the starting node
    /// </summary>
    /// <param name="StartingPoint"></param>
    /// <param name="EndPoint"></param>
    /// <returns></returns>
    private static List<PathfindingNode> RetracePath(PathfindingNode StartingPoint, PathfindingNode EndPoint)
    {
        List<PathfindingNode> path = new List<PathfindingNode>();
        PathfindingNode currentNode = EndPoint;
        while (currentNode != StartingPoint)
        {
            path.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }
        path.Reverse();
        return path;
    }
}