using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMovement : MonoBehaviour 
{
    public event Action<bool> OnWalkingStateChanged;
    public event Action<SlimeMovement> OnPathComplete;

    [SerializeField] protected float movementSpeed = 5f;
    [SerializeField] protected float turnSpeed = 20f;
    [SerializeField] protected float entityRadius = .25f;
    public float EntityRadius => entityRadius;
    public float MovementSpeed => movementSpeed;
    private List<Vector3> currentPathWaypoints;

    private bool isWalking;
    public bool IsWalking
    {
        get { return isWalking; }
        private set
        {
            isWalking = value;
            OnWalkingStateChanged?.Invoke(isWalking);

            if (!isWalking)
            {
                PathReservationManager.Instance.ClearReservationsForAgent(this);
                PathReservationManager.Instance.ReserveNodesForStaticEntity(this);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(nameof(LateStart));
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.1f);
        IsWalking = false;
    }

    public void RequestAndFollowPath(Vector3 destination, bool ignoreUnreachableTarget)
    {
        PathRequestManager.PathRequest pathRequest = new(this, destination, StartFollowPath, ignoreUnreachableTarget);
        PathRequestManager.Instance.RequestPath(pathRequest);
    }

    public void RequestAndFollowPath(Vector3 destination, bool ignoreUnreachableTarget, float distanceFromTarget)
    {
        PathRequestManager.PathRequest pathRequest = new(this, destination, StartFollowPathToTarget, ignoreUnreachableTarget, distanceFromTarget);
        PathRequestManager.Instance.RequestPath(pathRequest);
    }

    public void Stop()
    {
        if (IsWalking)
        {
            IsWalking = false;
            StopCoroutine(nameof(FollowPathCoroutine));
            StopCoroutine(nameof(FollowPathToTargetCoroutine));
        }
    }

    protected void StartFollowPath(PathRequestManager.ProcessedPath path)
    {
        if (path.Waypoints != null && path.Waypoints.Count > 0)
        {
            IsWalking = true;
            currentPathWaypoints = path.Waypoints;
            // currentPathWaypoints = GenerateSpline(SimplifyPath(path.Waypoints), 20);
            StopCoroutine(nameof(FollowPathCoroutine));
            StopCoroutine(nameof(FollowPathToTargetCoroutine));
            StartCoroutine(nameof(FollowPathCoroutine));
        }
        else
        {
            Debug.Log("Path is null!");
            IsWalking = false;
            OnPathComplete?.Invoke(this);
        }
    }

    protected void StartFollowPathToTarget(PathRequestManager.ProcessedPath path)
    {
        if (path.Waypoints != null && path.Waypoints.Count > 0)
        {
            IsWalking = true;
            currentPathWaypoints = path.Waypoints;
            // currentPathWaypoints = GenerateSpline(SimplifyPath(path.Waypoints), 20);
            StopCoroutine(nameof(FollowPathCoroutine));
            StopCoroutine(nameof(FollowPathToTargetCoroutine));
            StartCoroutine(nameof(FollowPathToTargetCoroutine), path.DistanceFromTarget);
        }
        else
        {
            Debug.Log("Path is null!");
            IsWalking = false;
            OnPathComplete?.Invoke(this);
        }
    }


    private IEnumerator FollowPathCoroutine()
    {
        int pathIndex = 0;
        float errorMargin = 0.05f;

        while (IsWalking)
        {
            if (Vector3.Distance(transform.position, currentPathWaypoints[pathIndex]) < errorMargin)
            {
                pathIndex++;

                if (pathIndex >= currentPathWaypoints.Count)
                {
                    IsWalking = false;
                    OnPathComplete?.Invoke(this);
                    yield break;
                }
            }

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(currentPathWaypoints[pathIndex] - transform.position),
                Time.deltaTime * turnSpeed);
            
            transform.position = Vector3.MoveTowards(
                transform.position,
                currentPathWaypoints[pathIndex],
                movementSpeed * Time.deltaTime);
            
            yield return null;
        }
    }

    private IEnumerator FollowPathToTargetCoroutine(float followDistance)
    {
        int pathIndex = 0;
        float distanceToTarget = Vector3.Distance(transform.position, currentPathWaypoints[currentPathWaypoints.Count - 1]);
        float errorMargin = 0.05f;

        while (IsWalking && Mathf.Abs(distanceToTarget - followDistance) > errorMargin)
        {
            Vector3 destination = currentPathWaypoints[pathIndex];
            if (Vector3.Distance(transform.position, destination) < errorMargin)
            {
                pathIndex++;

                if (pathIndex >= currentPathWaypoints.Count) break;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                movementSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(destination - transform.position),
                Time.deltaTime * turnSpeed);

            distanceToTarget = Vector3.Distance(transform.position, currentPathWaypoints[currentPathWaypoints.Count - 1]);
            yield return null;
        }

        IsWalking = false;
        OnPathComplete?.Invoke(this);
    }

    // private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    // {
    //     return 0.5f * (
    //         2f * p1 +
    //         (-p0 + p2) * t +
    //         (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
    //         (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
    //     );
    // }

    // private List<Vector3> GenerateSpline(List<Vector3> waypoints, int resolution = 10)
    // {
    //     List<Vector3> splinePoints = new();

    //     if (waypoints.Count < 2)
    //         return waypoints;

    //     for (int i = 0; i < waypoints.Count - 1; i++)
    //     {
    //         Vector3 p0 = i == 0 ? waypoints[i] : waypoints[i - 1];
    //         Vector3 p1 = waypoints[i];
    //         Vector3 p2 = waypoints[i + 1];
    //         Vector3 p3 = (i + 2 < waypoints.Count) ? waypoints[i + 2] : p2;

    //         for (int j = 0; j < resolution; j++)
    //         {
    //             float t = j / (float)resolution;
    //             splinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
    //         }
    //     }

    //     splinePoints.Add(waypoints[^1]); 
    //     return splinePoints;
    // }

    // List<Vector3> SimplifyPath(List<Vector3> path)
    // {
    //     if (path.Count < 2) return path;

    //     List<Vector3> simplified = new()
    //     {
    //         path[0]
    //     };

    //     Vector3 prevDirection = (path[1] - path[0]).normalized;

    //     for (int i = 1; i < path.Count - 1; i++)
    //     {
    //         Vector3 currDirection = (path[i + 1] - path[i]).normalized;
    //         if (Vector3.Angle(prevDirection, currDirection) > 0.01f)
    //         {
    //             simplified.Add(path[i]);
    //         }

    //         prevDirection = currDirection;
    //     }

    //     simplified.Add(path[^1]);
    //     return simplified;
    // }

    private void OnDrawGizmos()
    {
        if (currentPathWaypoints != null)
        {
            for (int i = 0; i < currentPathWaypoints.Count; i++)
            {
                Gizmos.color = Color.black;
                if (i != 0) Gizmos.DrawLine(currentPathWaypoints[i - 1], currentPathWaypoints[i]);
            }
        }
    }
}