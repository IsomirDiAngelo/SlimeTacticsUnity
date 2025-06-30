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
        IsWalking = false;
    }

    private void OnDestroy()
    {
        PathReservationManager.Instance.ClearReservationsForAgent(this);
    }

    public void RequestAndFollowPath(Vector3 destination, bool ignoreUnreachableTarget)
    {
        PathRequestManager.PathRequest pathRequest = new(this, destination, StartFollowPath, ignoreUnreachableTarget);
        PathRequestManager.Instance.RequestPath(pathRequest);
    }

    public void RequestAndFollowPath(Vector3 destination, bool ignoreUnreachableTarget, float distanceFromTarget)
    {
        PathRequestManager.PathRequest pathRequest = new(this, destination, StartFollowPath, ignoreUnreachableTarget, distanceFromTarget);
        PathRequestManager.Instance.RequestPath(pathRequest);
    }

    public void Stop()
    {
        if (IsWalking)
        {
            IsWalking = false;
            StopCoroutine(nameof(FollowPathCoroutine));
        }
    }

    protected void StartFollowPath(PathRequestManager.ResultPath path)
    {
        if (path.PathSteps != null && path.PathSteps.Count > 0)
        {
            IsWalking = true;
            currentPathWaypoints = PathProcessor.ProcessPath(path.PathSteps, this);
            StopCoroutine(nameof(FollowPathCoroutine));
            StartCoroutine(nameof(FollowPathCoroutine));
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