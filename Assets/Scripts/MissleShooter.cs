using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleShooter : MonoBehaviour
{
    public GameObject target;
    public GameObject missilePrefab;
    public float shootInterval;

    private float _timer;
    private void Start()
    {
        _timer = shootInterval;
    }
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            _timer = shootInterval;
            for (int i = 0; i < 2; i++)
            {
                GameObject missile = Instantiate(missilePrefab, transform.position, transform.rotation, transform);
                missile.GetComponent<MissleAim>().target = target;
                missile.GetComponent<Rigidbody2D>().AddForce(1000 * Random.insideUnitCircle);
            }
            
        }
    }
}
