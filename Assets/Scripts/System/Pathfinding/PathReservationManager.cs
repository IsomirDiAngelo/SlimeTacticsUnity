using System;
using System.Collections.Generic;
using UnityEngine;

public class PathReservationManager : MonoBehaviour
{
    public class ReservationAgent
    {
        public SlimeMovement Agent { get; private set; }
        public float EntryTime { get; private set; }
        public float ExitTime { get; private set; }
        public float Priority { get; private set; }

        public ReservationAgent(SlimeMovement agent, float entryTime, float exitTime)
        {
            Agent = agent;
            EntryTime = entryTime;
            ExitTime = exitTime;
        }

        public bool Overlaps(float otherEntryTime, float otherExitTime)
        {
            // if (ExitTime == float.MaxValue || otherExitTime == float.MaxValue)
            // {
            //     return true;
            // }

            return !(ExitTime <= otherEntryTime || EntryTime >= otherExitTime);
        }
    }
    public static PathReservationManager Instance { get; private set; }

    private Grid grid;

    private Dictionary<Node, List<ReservationAgent>> reservations;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        grid = GetComponent<Grid>();
        reservations = new();
    }

    public bool CanReserveNode(Node node, SlimeMovement agent, float entryTime, float exitTime)
    {
        // List<Node> affectedNodes = grid.GetCollidingNodes(node.WorldPosition, agent.EntityRadius);

        // foreach (Node affectedNode in affectedNodes)
        // {
            if (!reservations.TryGetValue(node, out List<ReservationAgent> reservationAgents))
            {
                return true;
            }

            foreach (ReservationAgent reservationAgent in reservationAgents)
            {
                if (agent != reservationAgent.Agent && reservationAgent.Overlaps(entryTime, exitTime))
                {
                    Debug.Log("Overlapping!");
                    return false;
                }
            }
        // }
        return true;
    }

    public void ReservePath(List<AStar.PathStep> pathSteps, SlimeMovement agent)
    {
        // Debug.Log("Reserving path for dynamic entity " + agent);

        for (int i = 0; i < pathSteps.Count; i++)
        {
            Node node = pathSteps[i].Node;
            float entryTime = pathSteps[i].EntryTime;
            float exitTime;

            if (i != pathSteps.Count - 1)
            {
                exitTime = pathSteps[i + 1].EntryTime;
            }
            else
            {
                exitTime = float.MaxValue;
            }

            ReserveNodesForDynamicEntity(node, agent, entryTime, exitTime);
        }
    }

    public void ClearReservationsForAgent(SlimeMovement agent)
    {
        List<Node> keysToRemove = new();
        // Debug.Log("Clearing reservations of agent " + agent);
        foreach (var kvp in reservations)
        {
            List<ReservationAgent> reservationAgents = kvp.Value;

            reservationAgents.RemoveAll(r => r.Agent == agent);

            if (reservationAgents.Count == 0)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            reservations.Remove(key);
        }
    }

    public void ReserveNodesForStaticEntity(SlimeMovement agent)
    {
        List<Node> nodes = grid.GetCollidingNodes(agent.transform.position, agent.EntityRadius);

        // Debug.Log("Reserving node for static entity " + node.WorldPosition + " " + agent);
        foreach (Node node in nodes)
        {

            if (!reservations.ContainsKey(node))
            {
                reservations[node] = new();
            }

            ReservationAgent reservationAgent = new(agent, 0f, float.MaxValue);
            reservations[node].Add(reservationAgent);
        }
    }
    
    public void ReserveNodesForDynamicEntity(Node centerNode, SlimeMovement agent, float entryTime, float exitTime)
    {
        List<Node> nodes = grid.GetCollidingNodes(centerNode.WorldPosition, agent.EntityRadius);

        foreach (Node node in nodes)
        {
            if (!reservations.ContainsKey(node))
            {
                reservations[node] = new();
            }

            ReservationAgent reservationAgent = new(agent, entryTime, exitTime);
            reservations[node].Add(reservationAgent);
        }
    }


    private void OnDrawGizmos()
    {
        if (reservations != null)
        {
            foreach (var node in reservations.Keys)
            {
                if (reservations[node].Count > 0)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(node.WorldPosition, new Vector3(0.2f, 0.2f, 0.2f));
                }
            }
        }
    }

}