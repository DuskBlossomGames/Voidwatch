using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

// TODO: make this a particle system instead
public class GenerateStars : MonoBehaviour
{
    public GameObject baseStar;
    public int maxFailAttempts;
    public int clusterDist;
    public int maxCluster;
    
    private void Awake()
    {
        var starPositions = new List<Vector3> { baseStar.transform.localPosition };
        var scale = transform.localScale;

        var failedAttempts = 0;
        while (failedAttempts < maxFailAttempts)
        {
            var position = starPositions[0] + new Vector3(
                Random.Range(-scale.x / 2, scale.x / 2),
                Random.Range(-scale.y / 2, scale.y / 2)
            );

            if (starPositions.Count(p =>
                    Vector2.Distance(p, position) < clusterDist) >= maxCluster)
            {
                failedAttempts++;
                continue;
            }

            Instantiate(baseStar, position, Quaternion.identity, transform);
            starPositions.Add(position);
        }
    }
}
