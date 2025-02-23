using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawnables.Asteroids
{
    public class AsteroidSwarmInit : MonoBehaviour
    {
        public float minDriftVel, maxDriftVel;

        public float size;
        public GameObject[] asteroids;

        private void Awake()
        {
            var driftVel = Random.Range(minDriftVel, maxDriftVel) *
                           (Quaternion.Euler(0, 0, 90 * Mathf.Sign(Random.value - 0.5f)) * transform.position.normalized);
            
            var origSize = size;
            while (size > 0)
            {
                var options = asteroids.Where(g => size >= g.transform.localScale.x).ToList();
                if (options.Count == 0) break;
                
                var obj = options[Random.Range(0, options.Count)];
                
                size -= obj.transform.localScale.x;

                var asteroid = Instantiate(obj, transform.position +
                                 (Vector3) (origSize/2*Random.insideUnitCircle), Quaternion.identity);
                asteroid.transform.SetParent(null);

                asteroid.GetComponent<AsteroidController>().startVel = driftVel;
            }

            Destroy(gameObject);
        }
    }
}