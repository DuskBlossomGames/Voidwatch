using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateStars : MonoBehaviour
{
    public GameObject baseStar;
    public int maxFailAttempts;
    public int clusterDist;
    public int maxCluster;
    
    private void Start()
    {
        var starPositions = new List<Vector2> { baseStar.transform.localPosition };

        var scale = transform.localScale;
        
        var failedAttempts = 0;
        while (failedAttempts < maxFailAttempts)
        {
            var position = new Vector3(
                Random.Range(-scale.x/2, scale.x/2),
                Random.Range(-scale.y/2, scale.y/2)
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
