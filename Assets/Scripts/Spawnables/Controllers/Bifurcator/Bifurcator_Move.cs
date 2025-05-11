using UnityEngine;

public class Bifurcator_Move : MonoBehaviour
{
    public float rotSpeed; // degrees per sec

    float _rot;
    
    void Start()
    {
        transform.position = new Vector3(0, 0, transform.position.z);
        transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);

        rotSpeed *= Mathf.Sign(Random.value - 0.5f);
    }

    
    void FixedUpdate()
    {
        transform.Rotate(0, 0, rotSpeed * Time.fixedDeltaTime);
    }
}
