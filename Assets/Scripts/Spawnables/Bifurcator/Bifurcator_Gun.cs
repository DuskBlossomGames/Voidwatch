using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class Bifurcator_Gun : MonoBehaviour
{
    public GameObject target;
    public EnemyGunHandler gun;
    MagicRigidBody _crb;

    Vector3 _opos;
    float _bulletvel;
    
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        _bulletvel = gun.ExpectedVelocity();
        _crb = GetComponent<MagicRigidBody>();
    }

    
    void Update()
    {
        Vector2 aprox_vel = (transform.position - _opos) / Time.deltaTime;
        _crb.velocity = aprox_vel;
        _opos = transform.position;

        var ppos = target.transform.position;
        var pvel = target.GetComponent<Rigidbody2D>().velocity;
        float dir = UtilFuncs.LeadShot(ppos - transform.position, pvel - aprox_vel, _bulletvel);
        transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * dir);

        gun.Shoot(0);
    }
}
