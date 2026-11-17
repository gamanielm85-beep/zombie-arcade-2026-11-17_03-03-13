using UnityEngine;

public class RampWaypoints : MonoBehaviour
{
    public static RampWaypoints Instance { get; private set; }

    public Transform bottom;
    public Transform top;
    public float platformHeightThreshold = 1.5f;
    public float waypointReachDistance = 1.5f;

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public Vector3 GetTarget(Vector3 from, Transform destination)
    {
        if (destination == null) return from;

        float heightDifference = destination.position.y - from.y;
        if (heightDifference > platformHeightThreshold && bottom != null && top != null)
            return Vector3.Distance(from, bottom.position) > waypointReachDistance ? bottom.position : top.position;

        if (heightDifference < -platformHeightThreshold && bottom != null && top != null)
            return Vector3.Distance(from, top.position) > waypointReachDistance ? top.position : bottom.position;

        return destination.position;
    }
}

public static class ZombieNav
{
    public static Vector3 GetTarget(Vector3 from, Transform player)
    {
        return RampWaypoints.Instance != null
            ? RampWaypoints.Instance.GetTarget(from, player)
            : player != null ? player.position : from;
    }
}
