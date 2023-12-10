using UnityEngine;
using Util;

public class Gravitatable : MonoBehaviour
{
    public GameObject gravitySource;
    private void FixedUpdate()
    {
        var rigid = transform.GetComponent<CustomRigidbody2D>();
        //F = ma = m (a _m/_s/_t) * (_t/_s)
        rigid.AddForce(rigid.mass * Time.fixedDeltaTime * gravitySource.GetComponent<GravityEmitter>().CalcGravAccel(rigid.position));
        }
}
