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

    public bool CanReservePosition(Vector3 worldPosition, SlimeMovement agent, float entryTime, float exitTime)
    {
        List<Node> affectedNodes = grid.GetCollidingNodes(worldPosition, agent.EntityRadius);

        foreach (Node affectedNode in affectedNodes)
        {
            if (!reservations.TryGetValue(affectedNode, out List<ReservationAgent> reservationAgents))
            {
                continue;
            }

            foreach (ReservationAgent reservationAgent in reservationAgents)
            {
                if (agent != reservationAgent.Agent && reservationAgent.Overlaps(entryTime, exitTime))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool CanReservePath(Vector3 from, Vector3 to, float entryTime, float exitTime, SlimeMovement agent)
    {
        float stepPercent = 0.1f;
        int steps = Mathf.CeilToInt(Vector3.Distance(from, to) / stepPercent);
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector3 samplePos = Vector3.Lerp(from, to, t);
            float sampleTime = Mathf.Lerp(entryTime, exitTime, t);

            if (!CanReservePosition(samplePos, agent, sampleTime, sampleTime + 0.01f))
            {
                return false;
            }
        }
        return true;
    }

    public void ReservePath(List<AStar.PathStep> pathSteps, SlimeMovement agent)
    {

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

            float bufferTime = .5f;

            ReserveNodesForDynamicEntity(node, agent, entryTime - bufferTime, exitTime + bufferTime);
        }
    }

    public void ClearReservationsForAgent(SlimeMovement agent)
    {
        List<Node> keysToRemove = new();

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

    public void Debug_PrintReservationsForAgent(SlimeMovement agent)
    {
        bool foundReservation = false;

        foreach (var kvp in reservations)
        {
            List<ReservationAgent> reservationAgents = kvp.Value;

            foreach (var rAgent in reservationAgents)
            {
                if (rAgent.Agent == agent)
                {
                    foundReservation = true;
                    Debug.Log("Agent " + agent.gameObject + " booked node" + kvp.Key.WorldPosition + " from " + rAgent.EntryTime + " to " + rAgent.ExitTime);
                }
            }
        }

        if (!foundReservation) Debug.Log("No reservations for agent " + agent.gameObject);

    }

    public void Debug_PrintNodeReservations(List<Node> nodes)
    {
        bool foundReservation = false;
        foreach (var kvp in reservations)
        {
            if (nodes.Contains(kvp.Key))
            {
                Debug.Log($"{kvp.Key.WorldPosition} reserved");
            }
        }
        

        if (!foundReservation) Debug.Log("No reservations for nodes");

    }

    public void ReserveNodesForStaticEntity(SlimeMovement agent)
    {
        List<Node> nodes = grid.GetCollidingNodes(agent.transform.position, agent.EntityRadius);

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