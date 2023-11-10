using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAtTargets : MonoBehaviour
{
    public GameObject bulletPrefab;
    public GameObject playArea;
    public GameObject gravitySource;
    public Transform target;
    float _rot = 0;
    float _countdown = 0;

    // Update is called once per frame
    void Update()
    {
        Vector2 diff = new Vector2(target.position.x - transform.parent.position.x, target.position.y - transform.parent.position.y);

        if((diff.x*transform.position.x + diff.y * transform.position.y) > 0)
        {
            _countdown -= Time.deltaTime;
            _rot = Mathf.Atan2(diff.y, diff.x);
        } else
        {
            _countdown = 1;
            _rot = Mathf.Pow(.5f,Time.deltaTime) * (_rot - Mathf.PI / 2)+Mathf.PI/2;
        }

        if (_countdown < 0)
        {
            _countdown = .05f;
            var bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            bullet.GetComponent<DestroyOffScreen>().playArea = playArea;
            bullet.GetComponent<Gravitatable>().gravitySource = gravitySource;
            bullet.GetComponent<Rigidbody2D>().AddRelativeForce(new Vector2(0, 5000));
        }

        
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, -90 + Mathf.Rad2Deg * _rot));
        transform.localPosition = new Vector3(.211f * Mathf.Cos(_rot), .211f * Mathf.Sin(_rot) +.099f, transform.localPosition.z);
    }
}
