using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gravitatable : MonoBehaviour
{
    public GameObject gravitySource;
    private void FixedUpdate()
    {
        Rigidbody2D rigid = transform.GetComponent<Rigidbody2D>();
        //F = ma = m (a _m/_s/_t) * (_t/_s)
        rigid.AddForce(rigid.mass * Time.fixedDeltaTime * gravitySource.GetComponent<GravityEmitter>().CalcGravAccel(rigid.position));
        }
}
