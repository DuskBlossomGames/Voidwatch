using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spawnables.Controllers.Asteroids
{
    public class AsteroidSwarmInit : MonoBehaviour
    {
        public float spreadMultiplier;
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
                var ideal = options.Where(g => origSize / 3f >= g.transform.localScale.x).ToList();
                if (ideal.Count > 0) options = ideal;
                
                var obj = options[Random.Range(0, options.Count)];
                
                size -= obj.transform.localScale.x;

                var asteroid = Instantiate(obj, transform.position +
                                 (Vector3) (spreadMultiplier*origSize*Random.insideUnitCircle), Quaternion.identity);

                asteroid.GetComponent<AsteroidController>().startVel = driftVel;
            }

            Destroy(gameObject);
        }
    }
}