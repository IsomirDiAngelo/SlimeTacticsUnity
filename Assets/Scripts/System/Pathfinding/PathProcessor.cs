using System.Collections.Generic;
using UnityEngine;

public class PathProcessor
{
    public static List<Vector3> ProcessPath(List<AStar.PathStep> pathSteps, SlimeMovement agent)
    {
        return GenerateSpline(SmoothPath(pathSteps, LayerMask.GetMask("NotWalkable"), agent));
    }
    private static List<Vector3> SmoothPath(List<AStar.PathStep> pathSteps, LayerMask obstacleMask, SlimeMovement agent)
    {
        List<Vector3> smoothPath = new();
        int current = 0;
        while (current != pathSteps.Count - 1)
        {
            smoothPath.Add(pathSteps[current].Node.WorldPosition);
            int next = pathSteps.Count - 1;
            for (int i = pathSteps.Count - 1; i > current; i--)
            {
                bool isSubPathWalkable = !Physics.Linecast(pathSteps[current].Node.WorldPosition, pathSteps[i].Node.WorldPosition, obstacleMask);

                float exitTime = i + 1 < pathSteps.Count ? pathSteps[i + 1].EntryTime : float.MaxValue;
                bool canReserveSubPath = PathReservationManager.Instance.CanReservePath(pathSteps[current].Node.WorldPosition, pathSteps[i].Node.WorldPosition, pathSteps[current].EntryTime, exitTime, agent);

                if (isSubPathWalkable && canReserveSubPath)
                {
                    next = i;
                    break;
                }
            }
            current = next;
        }
        smoothPath.Add(pathSteps[pathSteps.Count - 1].Node.WorldPosition);
        return smoothPath;
    }
    
    private static List<Vector3> GenerateSpline(List<Vector3> waypoints, int resolution = 10)
    {
        List<Vector3> splinePoints = new();

        if (waypoints.Count < 2)
            return waypoints;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Vector3 p0 = i == 0 ? waypoints[i] : waypoints[i - 1];
            Vector3 p1 = waypoints[i];
            Vector3 p2 = waypoints[i + 1];
            Vector3 p3 = (i + 2 < waypoints.Count) ? waypoints[i + 2] : p2;

            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;
                splinePoints.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        splinePoints.Add(waypoints[^1]);
        return splinePoints;
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

}