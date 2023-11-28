using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissilerPathfinding : MonoBehaviour
{
    public GameObject target;
    public float minDist;

    private Vector3 _tar;
    private bool _moving = false;
    private Rigidbody2D _rigid;
    
    void Start()
    {
        _rigid = GetComponent<Rigidbody2D>();
    }

    
    void Update()
    {
        if (!_moving) {
            if ((transform.position - target.transform.position).sqrMagnitude > minDist * minDist)
            {
                float mag = target.transform.position.magnitude;
                _tar = mag * (transform.position + target.transform.position).normalized;
                _moving = true;
            }
        } else
        {
            Vector3 dif = _tar - transform.position;
            if (dif.sqrMagnitude < 10)
            {
                _moving = false;
            }
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dif.y,dif.x));
            _rigid.AddRelativeForce(new Vector2(10,0));
            _rigid.velocity = Vector2.ClampMagnitude(_rigid.velocity, 10);
        }
    }
}
