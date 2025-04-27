using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class Bifurcator_Move : MonoBehaviour
{
    public float rotSpeed = .1f; // radians per sec

    float _rot;
    
    void Start()
    {
        transform.position = new Vector3(0, 0, transform.position.z);
        transform.rotation = Quaternion.Euler(0, 0, Random.value * 360);

        rotSpeed *= Mathf.Sign(Random.value - 0.5f);
    }

    
    void Update()
    {
        transform.Rotate(0, 0, rotSpeed);
    }
}
