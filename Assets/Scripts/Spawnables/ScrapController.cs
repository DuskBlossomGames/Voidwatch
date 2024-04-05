using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class ScrapController : MonoBehaviour
{
    CustomRigidbody2D _crb;
    GameObject _player;
    


    private void Start()
    {
        _crb = GetComponent<CustomRigidbody2D>();
        _player = GameObject.FindGameObjectWithTag("Player");
        
    }

    void Update()
    {
        Vector2 norm = ((Vector2)_player.transform.position - (Vector2)transform.position).normalized;
        _crb.velocity = norm * _crb.velocity.magnitude;
        _crb.velocity += 10 * norm * Time.deltaTime;
    }

}
