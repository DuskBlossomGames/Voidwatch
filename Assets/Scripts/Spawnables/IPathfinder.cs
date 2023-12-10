using UnityEngine;

public interface IPathfinder
{
    public Vector2 PathDirNorm(Vector2 currentPosition, Vector2 targetPosition);
    public float PathAngleDeg(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 dir = PathDirNorm(currentPosition, targetPosition);
        return Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
    }
    public float PathAngleRad(Vector2 currentPosition, Vector2 targetPosition)
    {
        Vector2 dir = PathDirNorm(currentPosition, targetPosition);
        return Mathf.Atan2(dir.y, dir.x);
    }
}
