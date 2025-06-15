using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Vector2 worldGridSize;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float nodeRadius = 0.5f;
    [SerializeField] private LayerMask notWalkableMask;
    private Node[,] grid;
    private List<Node> temporaryNodes;

    public float NodeRadius => nodeRadius;
    public LayerMask NotWalkableMask => notWalkableMask;

    private void Start()
    {
        ClearTemporaryNodes();
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        gridSize.x = Mathf.RoundToInt(worldGridSize.x / nodeRadius);
        gridSize.y = Mathf.RoundToInt(worldGridSize.y / nodeRadius);
        grid = new Node[gridSize.x, gridSize.x];

        Vector3 worldBottomLeft = transform.position - Vector3.right * worldGridSize.x / 2 - Vector3.forward * worldGridSize.y / 2;
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeRadius + nodeRadius / 2) + Vector3.forward * (y * nodeRadius + nodeRadius / 2);
                bool isWalkable = !Physics.CheckSphere(worldPoint, nodeRadius, notWalkableMask);
                grid[x, y] = new Node(isWalkable, worldPoint, new Vector2Int(x, y));
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(worldGridSize.x, 1, worldGridSize.y));

        if (grid != null)
        {
            foreach (Node node in grid)
            {
                Gizmos.color = node.IsWalkable ? Color.green : Color.red;
                Gizmos.DrawCube(node.WorldPosition, new Vector3(0.1f, 0.1f, 0.1f));
            }
        }

    }

    public void ResetNodes()
    {
        foreach (Node node in grid)
        {
            node.HCost = float.MaxValue;
            node.GCost = 0f;
            node.Parent = null;
        }
    }

    public Node GetNodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + worldGridSize.x / 2) / worldGridSize.x;
        float percentY = (worldPosition.z + worldGridSize.y / 2) / worldGridSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSize.x - 1) * percentX);
        int y = Mathf.RoundToInt((gridSize.y - 1) * percentY);

        return grid[x, y];
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int neighbourPos = new(node.GridPosition.x + x, node.GridPosition.y + y);
                
                if (x == 0 && y == 0)
                    // Skip the current node
                    continue;

                if (AreGridCoordinatesValid(neighbourPos.x, neighbourPos.y))
                {
                    Node neighbour = grid[neighbourPos.x, neighbourPos.y];

                    // if (dynamicallyBlockedNodes.Contains(neighbour)) continue;

                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    public Node AddTemporaryNode(Vector3 worldPosition)
    {
        bool isWalkable = !Physics.CheckSphere(worldPosition, nodeRadius, notWalkableMask);
        Node temporaryNode = new(isWalkable, worldPosition, Vector2Int.zero);

        temporaryNodes.Add(temporaryNode);

        return temporaryNode;
    }

    public Node FindWalkableNodeAroundTarget(Vector3 targetPosition, float distanceFromTarget, int sampleSize, float entryTime, float exitTime)
    {
        float stepAngle = 360f / sampleSize;
        for (int i = 0; i < sampleSize; i++)
        {
            float angle = stepAngle * i * Mathf.Deg2Rad;
            Vector3 samplePosition = targetPosition + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distanceFromTarget;

            bool isWalkable = !Physics.CheckSphere(samplePosition, nodeRadius, notWalkableMask);
        }
        return null;
    }

    // public List<Node> GetCollidingNodes(Transform obstacle)
    // {
    //     List<Node> result = new();
    //     Node centerNode = GetNodeFromWorldPoint(obstacle.position);

    //     float obstacleRadius = .25f;
    //     if (obstacle.TryGetComponent(out SlimeMovement slimeMovement))
    //     {
    //         obstacleRadius = slimeMovement.EntityRadius;
    //     }

    //     int range = Mathf.FloorToInt(obstacleRadius / nodeRadius);
    //     Debug.Log(range);

    //     for (int x = -range; x <= range; x++)
    //     {
    //         for (int y = -range; y <= range; y++)
    //         {
    //             int nodeX = centerNode.GridPosition.x + x;
    //             int nodeY = centerNode.GridPosition.y + y;

    //             if (AreGridCoordinatesValid(nodeX, nodeY))
    //             {
    //                 result.Add(grid[nodeX, nodeY]);
    //             }
    //         }
    //     }

    //     foreach (Node temporaryNode in temporaryNodes)
    //     {
    //         float nodeDiameter = 2 * nodeRadius;
    //         bool isNearby = Vector3.Distance(obstacle.position, temporaryNode.WorldPosition) < nodeDiameter;

    //         if (isNearby)
    //         {
    //             result.Add(temporaryNode);
    //         }
    //     }

    //     return result;
    // }

    public List<Node> GetCollidingNodes(Vector3 obstaclePosition, float obstacleRadius)
    {
        List<Node> result = new();
        Node centerNode = GetNodeFromWorldPoint(obstaclePosition);

        int range = Mathf.FloorToInt(obstacleRadius / nodeRadius);

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                int nodeX = centerNode.GridPosition.x + x;
                int nodeY = centerNode.GridPosition.y + y;

                if (AreGridCoordinatesValid(nodeX, nodeY))
                {
                    result.Add(grid[nodeX, nodeY]);
                }
            }
        }

        return result;
    }

    private bool AreGridCoordinatesValid(int x, int y)
    {
        return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
    }

    public void ClearTemporaryNodes()
    {
        temporaryNodes = new();
    }
}
