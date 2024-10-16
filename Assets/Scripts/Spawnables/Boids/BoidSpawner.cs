using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    public int boidCount;
    
    public void Ready()
    {
        GameObject sample = transform.GetChild(0).gameObject;
        for (int i = 0; i < boidCount-1; i++)
        {
            GameObject newChild = Instantiate(sample, transform);
            newChild.transform.position += new Vector3(Random.Range(-10, 10), Random.Range(-10, 10));
            newChild.GetComponent<BoidHandler>().original = false;
        }
    }
    
    private void Update()
    {
        if (transform.childCount == 0) Destroy(gameObject);
    }
}
