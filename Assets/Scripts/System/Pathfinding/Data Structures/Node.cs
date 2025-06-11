using System;
using UnityEngine;

public class Node : IComparable<Node>
{
    public bool IsWalkable { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public Node Parent { get; set; }

    public Node(bool isWalkable, Vector3 worldPosition, Vector2Int gridPosition)
    {
        IsWalkable = isWalkable;
        WorldPosition = worldPosition;
        GridPosition = gridPosition;
    }

    public int CompareTo(Node otherNode) {
        int compare = FCost.CompareTo(otherNode.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(otherNode.HCost);
        }
        return compare;
    }
}
