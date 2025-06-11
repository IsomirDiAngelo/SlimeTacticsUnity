using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public struct PathStep
    {
        public Node Node { get; private set; }
        public float EntryTime { get; private set; }

        public PathStep(Node node, float entryTime)
        {
            Node = node;
            EntryTime = entryTime;
        }
    }
    private Grid grid;
    private Dictionary<Node, float> entryTimes;
    public event Action<List<PathStep>> OnPathFound;

    private void Start()
    {
        grid = GetComponent<Grid>();

        ResetEntryTimes();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos, SlimeMovement agent, bool ignoreUnreachableTarget)
    {
        Node startNode = grid.GetNodeFromWorldPoint(startPos);
        Node targetNode;

        // if (addTargetNode)
        // {
        //     targetNode = grid.AddTemporaryNode(targetPos);
        // }
        // else
        // {
        targetNode = grid.GetNodeFromWorldPoint(targetPos);
        // }

        StopCoroutine(nameof(FindPath));
        StartCoroutine(FindPath(startNode, targetNode, agent, Time.time, ignoreUnreachableTarget));
    }

    private IEnumerator FindPath(Node startNode, Node targetNode, SlimeMovement agent, float startTime, bool ignoreUnreachableTarget)
    {
        bool isPathFound = false;
        List<Node> targetNodes = grid.GetCollidingNodes(targetNode.WorldPosition, agent.EntityRadius);
        float estimatedTargetNodeEntryTime = GetDistance(startNode, targetNode) / agent.MovementSpeed;

        bool isTargetNodeFree = PathReservationManager.Instance.CanReserveNode(targetNode, agent, estimatedTargetNodeEntryTime, float.MaxValue);

        if ((isTargetNodeFree && targetNode.IsWalkable) || ignoreUnreachableTarget)
        {
            BinaryHeap<Node> openSet = new();
            HashSet<Node> closedSet = new();
            openSet.Insert(startNode);
            entryTimes.Add(startNode, startTime);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.ExtractMin();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    isPathFound = true;
                    break;
                }

                foreach (Node neighbourNode in grid.GetNeighbours(currentNode))
                {
                    if (!neighbourNode.IsWalkable || closedSet.Contains(neighbourNode))
                        continue;

                    float travelTime = GetDistance(currentNode, neighbourNode) / agent.MovementSpeed;
                    float entryTime;

                    if (entryTimes.TryGetValue(currentNode, out float currentNodeEntryTime))
                    {
                        entryTime = currentNodeEntryTime + travelTime;
                    }
                    else
                    {
                        entryTime = float.MaxValue;
                        Debug.Log("Entry time for node at position " + currentNode.WorldPosition
                            + " was not set, final path may be incorrect!");
                    }

                    float estimatedExitTime = entryTime + travelTime; // Most nodes are equidistant, except for temporary nodes who may cause issues

                    if (targetNodes.Contains(neighbourNode)) estimatedExitTime = float.MaxValue;

                    if ((!targetNodes.Contains(neighbourNode) || !ignoreUnreachableTarget) && !PathReservationManager.Instance.CanReserveNode(neighbourNode, agent, entryTime, estimatedExitTime))
                    {
                        // Debug.Log("Skipping node: " + neighbourNode.WorldPosition + " with target " + targetNode.WorldPosition);
                        continue;
                    }

                    float newCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbourNode);
                    if (newCostToNeighbour < neighbourNode.GCost || !openSet.Contains(neighbourNode))
                    {
                        neighbourNode.GCost = newCostToNeighbour;
                        neighbourNode.HCost = GetDistance(neighbourNode, targetNode);
                        neighbourNode.Parent = currentNode;

                        if (!entryTimes.TryAdd(neighbourNode, entryTime))
                        {
                            entryTimes[neighbourNode] = entryTime;
                        }

                        if (!openSet.Contains(neighbourNode))
                        {
                            openSet.Insert(neighbourNode);
                        }
                        else
                        {
                            openSet.UpdateItem(neighbourNode);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("Target node unreachable!");
        }


        if (isPathFound)
        {
            List<PathStep> pathSteps = RetracePath(startNode, targetNode);
            OnPathFound?.Invoke(pathSteps);
        }
        else
        {
            OnPathFound?.Invoke(null);
        }

        grid.ClearTemporaryNodes();
        ResetEntryTimes();
        yield return null;
    }

    private List<PathStep> RetracePath(Node startNode, Node endNode)
    {
        List<PathStep> path = new();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(new PathStep(currentNode, entryTimes[currentNode]));
            currentNode = currentNode.Parent;
        }

        path.Reverse();
        return path;
    }

    private void ResetEntryTimes()
    {
        entryTimes = new();
    }

    private float GetDistance(Node nodeA, Node nodeB)
    {
        return Vector3.Distance(nodeA.WorldPosition, nodeB.WorldPosition);
    }
}