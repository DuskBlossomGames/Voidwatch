using System.Collections.Generic;
using UnityEngine;
using Util;

public class BoidHandler : MonoBehaviour
{
    public float visualRangel;
    public float protectedRange;
    public float matchingFactor;
    public float centeringFactor;
    public float avoidFactor;
    public float targettingFactor;
    public float maxSpeed;
    public float minSpeed;
    public GameObject target;
    public GameObject gravitySource;
    public float shootDist;
    private CustomRigidbody2D _rigidbody2D;
    private GunHandler _gun;
    public bool original = true;

    private void Start()
    {
        _gun = GetComponent<GunHandler>();
        _rigidbody2D = transform.GetComponent<CustomRigidbody2D>();
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        if (gravitySource == null) gravitySource = GameObject.FindGameObjectWithTag("GravitySource");
        if (original)
        {
            original = false;
            transform.parent.GetComponent<BoidSpawner>().Ready();
        }
    }

    float SqrHypot(Vector2 v, Vector2 w)
    {
        return (v.x - w.x) * (v.x - w.x) + (v.y - w.y) * (v.y - w.y);
    }

    void Shoot()
    {
        Vector2 diff = new Vector2(target.transform.position.x - transform.position.x, target.transform.position.y - transform.position.y);
        Vector2 relVel = target.GetComponent<CustomRigidbody2D>().velocity - _rigidbody2D.velocity;
        float angle = leadShot(diff, relVel, _gun.ExpectedVelocity());
        //Debug.LogErrorFormat("Errored angle = {0}", angle);
        Quaternion rot = Quaternion.Euler(new Vector3(0, 0, -90 + Mathf.Rad2Deg * angle));
        transform.rotation = rot;


        if (diff.sqrMagnitude < shootDist * shootDist)
        {
            _gun.Shoot(0);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        List<GameObject> otherBoids = new List<GameObject>() ;
        List<GameObject> nearBoids = new List<GameObject>();

        Vector2 avgVel = new Vector2();
        Vector2 avgPos = new Vector2();
        Vector2 pushaway = new Vector2();

        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform child = transform.parent.GetChild(i);
            if (SqrHypot(transform.position, child.transform.position) < visualRangel)
            {
                otherBoids.Add(child.gameObject);
                avgVel += child.GetComponent<CustomRigidbody2D>().velocity;
                avgPos += child.GetComponent<CustomRigidbody2D>().position;

                if (SqrHypot(transform.position, child.transform.position) < protectedRange)
                {
                    nearBoids.Add(child.gameObject);
                    pushaway += (_rigidbody2D.position - child.GetComponent<CustomRigidbody2D>().position) * avoidFactor;
                }
            }
        }
        if (otherBoids.Count > 0)
        {
            avgVel /= otherBoids.Count;
            avgPos /= otherBoids.Count;

            _rigidbody2D.velocity += pushaway;
            _rigidbody2D.velocity += (avgVel - _rigidbody2D.velocity) * matchingFactor;
            _rigidbody2D.velocity += (avgPos - _rigidbody2D.position) * centeringFactor;
            _rigidbody2D.velocity += ((Vector2)target.transform.position - _rigidbody2D.position) * targettingFactor;

            float sqrSpeed = _rigidbody2D.velocity.sqrMagnitude;
            if (sqrSpeed > maxSpeed * maxSpeed)
            {
                Vector2 uVel = _rigidbody2D.velocity.normalized;
                _rigidbody2D.velocity = uVel * maxSpeed;
            } else if (sqrSpeed < minSpeed * minSpeed)
            {
                Vector2 uVel = _rigidbody2D.velocity.normalized;
                _rigidbody2D.velocity = uVel * minSpeed;
            }

            Shoot();

        } else
        {
            _rigidbody2D.velocity = new Vector2();
        }
    }
    float leadShot(Vector2 relPos, Vector2 relVel, float bulletVel)
    {
        float a = bulletVel * bulletVel - relVel.sqrMagnitude;
        float b = 2 * Vector2.Dot(relPos, relVel);
        float c = relPos.sqrMagnitude;

        if(b * b + 4 * a * c < 0 || a==0){
            return 0;
        }

        float colTime = (b + Mathf.Sqrt(b * b + 4 * a * c)) / (2 * a);
        Vector2 colPos = relPos + colTime * relVel;
        return Mathf.Atan2(colPos.y, colPos.x);
    }
}
