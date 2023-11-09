using UnityEngine;

namespace Player
{
    public class Shoot : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public GameObject playArea;
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                bullet.GetComponent<DestroyOffScreen>().playArea = playArea;
                bullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, 700));
            }
        }
    }
}
