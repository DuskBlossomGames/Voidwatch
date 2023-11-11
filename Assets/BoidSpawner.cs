using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public int boidCount;
    // Start is called before the first frame update
    void Start()
    {
        GameObject sample = transform.GetChild(0).gameObject;
        for (int i = 0; i < boidCount; i++)
        {
            GameObject newChild = Instantiate(sample, transform);
            newChild.transform.position += new Vector3(Random.Range(-10, 10), Random.Range(-10, 10));
            //newChild.GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 100));

        }
        Destroy(sample);
    }
}
