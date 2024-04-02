using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRVGun : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject target;
    float _bulletvel;
    Util.CustomRigidbody2D _crb;
    public EnemyGunHandler gun;

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        _bulletvel = gun.ExpectedVelocity();
        _crb = GetComponent<Util.CustomRigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var ppos = target.transform.position;
        var pvel = target.GetComponent<Rigidbody2D>().velocity;

        float dir = Util.UtilFuncs.LeadShot(ppos - transform.position, pvel - _crb.velocity, _bulletvel);
        float tardir = Mathf.Rad2Deg * dir - 90;

        gun.Shoot(tardir - transform.rotation.eulerAngles.z);
    }
}
