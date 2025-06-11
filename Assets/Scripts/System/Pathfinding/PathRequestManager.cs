using System;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{

    public static PathRequestManager Instance { get; private set; }

    public struct PathRequest
    {
        public Vector3 StartPosition { get; private set; }
        public Vector3 EndPosition { get; private set; }
        public Action<ProcessedPath> Callback { get; private set; }
        public bool IgnoreUnreachableTarget { get; private set; }
        public float DistanceFromTarget { get; private set; }
        public SlimeMovement Agent { get; private set; }

        public PathRequest(SlimeMovement agent, Vector3 endPosition, Action<ProcessedPath> callback, bool ignoreUnreachableTarget)
        {
            Agent = agent;
            StartPosition = agent.transform.position;
            EndPosition = endPosition;
            Callback = callback;
            IgnoreUnreachableTarget = ignoreUnreachableTarget;
            DistanceFromTarget = 0f;
        }

        public PathRequest(SlimeMovement agent, Vector3 endPosition, Action<ProcessedPath> callback, bool ignoreUnreachableTarget, float distanceFromTarget)
        {
            Agent = agent;
            StartPosition = agent.transform.position;
            EndPosition = endPosition;
            Callback = callback;
            IgnoreUnreachableTarget = ignoreUnreachableTarget;
            DistanceFromTarget = distanceFromTarget;
        }
    }

    public struct ProcessedPath
    {
        public List<Vector3> Waypoints;
        public float DistanceFromTarget;

        public ProcessedPath(List<AStar.PathStep> pathSteps, float distanceFromTarget)
        {
            Waypoints = new();
            if (pathSteps != null)
            {
                foreach (AStar.PathStep pathStep in pathSteps)
                {
                    Waypoints.Add(pathStep.Node.WorldPosition);
                }
            }
            DistanceFromTarget = distanceFromTarget;
        }
    }

    private Queue<PathRequest> pathRequestQueue;
    private bool isProcessingPath;
    private AStar aStar;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Instance.pathRequestQueue = new();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        aStar = GetComponent<AStar>();
        aStar.OnPathFound += AStar_OnPathFound;

        isProcessingPath = false;
    }

    private void AStar_OnPathFound(List<AStar.PathStep> pathSteps)
    {
        PathRequest pathRequest = Instance.pathRequestQueue.Dequeue();
        if (pathSteps != null) PathReservationManager.Instance.ReservePath(pathSteps, pathRequest.Agent);
        ProcessedPath path = new(pathSteps, pathRequest.DistanceFromTarget);
        pathRequest.Callback(path);
        TryProcessNextPath();
    }

    private void TryProcessNextPath()
    {
        if (!isProcessingPath && Instance.pathRequestQueue.Count > 0)
        {
            isProcessingPath = true;

            PathRequest currentPathRequest = Instance.pathRequestQueue.Peek();

            aStar.StartFindPath(currentPathRequest.StartPosition, currentPathRequest.EndPosition, currentPathRequest.Agent, currentPathRequest.IgnoreUnreachableTarget);
        }
        else if (Instance.pathRequestQueue.Count == 0)
        {
            isProcessingPath = false;
        }

        if (Instance.pathRequestQueue.Count >= 10)
        {
            Debug.Log("Path request queue is too big!");
        }
    }

    public void RequestPath(PathRequest pathRequest)
    {
        Instance.pathRequestQueue.Enqueue(pathRequest);
        TryProcessNextPath();
    }
}