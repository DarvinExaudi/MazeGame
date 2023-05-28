using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode
{
    public Vector2Int position;
    public List<PathfindingNode> neighbors;

    public PathfindingNode(Vector2Int pos)
    {
        position = pos;
        neighbors = new List<PathfindingNode>();
    }
}
