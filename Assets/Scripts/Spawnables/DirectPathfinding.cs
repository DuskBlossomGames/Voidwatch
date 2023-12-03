using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectPathfinding : MonoBehaviour, IPathfinder
{
    public Vector2 PathDirNorm(Vector2 currentPosition, Vector2 targetPosition)
    {
        return (targetPosition-currentPosition).normalized;
    }
}
