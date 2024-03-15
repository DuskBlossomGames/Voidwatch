using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterX : MonoBehaviour
{
    public float killTime;
    float _timeToDeath;

    private void Start()
    {
        _timeToDeath = killTime;
    }
    // Update is called once per frame
    void Update()
    {
        _timeToDeath -= Time.deltaTime;
        if(_timeToDeath <= 0)
        {
            Destroy(gameObject);
        }
        
    }
}
