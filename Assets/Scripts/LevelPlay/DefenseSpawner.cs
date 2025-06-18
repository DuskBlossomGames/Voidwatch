using UnityEngine;

namespace LevelPlay
{
    public class DefenseSpawner : MonoBehaviour
    {
        public int defCount;

        Vector2 rot(Vector2 vec, float angle)
        {
            return new Vector2(vec.x*Mathf.Cos(angle)+vec.y*Mathf.Sin(angle), -vec.x * Mathf.Sin(angle) + vec.y * Mathf.Cos(angle));
        }

        void Start()
        {
            GameObject sample = transform.GetChild(0).gameObject;
            for (int i = 0; i < defCount; i++)
            {
                GameObject newChild = Instantiate(sample, transform);
                float angle = (float) i / defCount * (2 * Mathf.PI);
                newChild.transform.position = rot(sample.transform.position,angle);
                newChild.transform.rotation = Quaternion.Euler(0, 0, -Mathf.Rad2Deg * angle);
                //newChild.GetComponent<CustomRigidbody2D>().AddForce(new Vector2(0, 100));

            }
            Destroy(sample);
        }
    }
}
