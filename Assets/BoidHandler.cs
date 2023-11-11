using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    float SqrHypot(Vector2 v, Vector2 w)
    {
        return (v.x - w.x) * (v.x - w.x) + (v.y - w.y) * (v.y - w.y);
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
                avgVel += child.GetComponent<Rigidbody2D>().velocity;
                avgPos += child.GetComponent<Rigidbody2D>().position;

                if (SqrHypot(transform.position, child.transform.position) < protectedRange)
                {
                    nearBoids.Add(child.gameObject);
                    pushaway += (transform.GetComponent<Rigidbody2D>().position - child.GetComponent<Rigidbody2D>().position) * avoidFactor;
                }
            }
        }
        if (otherBoids.Count > 0)
        {
            avgVel /= otherBoids.Count;
            avgPos /= otherBoids.Count;

            transform.GetComponent<Rigidbody2D>().velocity += pushaway;
            transform.GetComponent<Rigidbody2D>().velocity += (avgVel - transform.GetComponent<Rigidbody2D>().velocity) * matchingFactor;
            transform.GetComponent<Rigidbody2D>().velocity += (avgPos - transform.GetComponent<Rigidbody2D>().position) * centeringFactor;
            transform.GetComponent<Rigidbody2D>().velocity += ((Vector2)target.transform.position - transform.GetComponent<Rigidbody2D>().position) * targettingFactor;

            float sqrSpeed = transform.GetComponent<Rigidbody2D>().velocity.sqrMagnitude;
            if (sqrSpeed > maxSpeed * maxSpeed)
            {
                Vector2 uVel = transform.GetComponent<Rigidbody2D>().velocity.normalized;
                transform.GetComponent<Rigidbody2D>().velocity = uVel * maxSpeed;
            } else if (sqrSpeed < minSpeed * minSpeed)
            {
                Vector2 uVel = transform.GetComponent<Rigidbody2D>().velocity.normalized;
                transform.GetComponent<Rigidbody2D>().velocity = uVel * minSpeed;
            }

        } else
        {
            transform.GetComponent<Rigidbody2D>().velocity = new Vector2();
        }
    }
}
