using UnityEngine;

namespace Player
{
    public class Shoot : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public float playRadius;
        public GameObject gravitySource;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                for (int i = 0; i < 5; i++)
                {
                    var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                    bullet.GetComponent<DestroyOffScreen>().playRadius = playRadius;
                    bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;
                    bullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, 1700)+100*Random.insideUnitCircle);
                }
                
            }
        }
    }
}
