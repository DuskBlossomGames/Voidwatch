using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaserBehavior : MonoBehaviour
{
    public GameObject target;
    public DirectPathfinding pathfinder;
    public float attackDist;
    public float accel;
    public float maxSpeed;

    private Vector2 _movDir;
    private float _currSpeed;

    private void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        Vector2 diff = (target.transform.position - transform.position);
        if (diff.sqrMagnitude < attackDist * attackDist)
        {
            //attack
        } else
        {
            float halfDistV = diff.magnitude / 2;
            Vector2 e1 = (Vector2)transform.position + halfDistV * GetComponent<Rigidbody2D>().velocity.normalized;
            Vector2 e2 = (Vector2)target.transform.position - halfDistV * target.GetComponent<Rigidbody2D>().velocity.normalized;
            Vector2 em = (e1 + e2) / 2;
            _movDir = pathfinder.PathDirNorm(transform.position, em);
            _currSpeed = Mathf.Min(maxSpeed, _currSpeed + accel * Time.deltaTime);
            GetComponent<Rigidbody2D>().velocity = _movDir * _currSpeed;
        }
    }
}
