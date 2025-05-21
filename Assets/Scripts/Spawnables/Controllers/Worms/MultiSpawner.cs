using UnityEngine;

public class MultiSpawner : MonoBehaviour
{
    public int childcount;

    void Start()
    {
        for (int i = 0; i < childcount-1; i++)
        {
            var nchild = Instantiate(transform.GetChild(0), parent: transform);
            nchild.transform.position += 10 * (Vector3)Random.insideUnitCircle;
        }
    }

    private void Update()
    {
        if(transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
