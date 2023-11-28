using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleShooter : MonoBehaviour
{
    public GameObject target;
    public GameObject missilePrefab;
    public float shootInterval;
    public float engageDist;
    public int amt = 3;

    private float _timer;
    private void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        _timer = shootInterval;
    }
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            _timer = shootInterval;
            if ((target.transform.position - transform.position).sqrMagnitude < engageDist * engageDist)
            {
                for (int i = 0; i < amt; i++)
                {
                    GameObject missile = Instantiate(missilePrefab, transform.position, transform.rotation);
                    missile.GetComponent<MissleAim>().target = target;
                    missile.GetComponent<Rigidbody2D>().AddForce(1000 * Random.insideUnitCircle);
                }
            }
            
        }
    }
}
