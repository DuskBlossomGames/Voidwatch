using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class Bifurcator_Move : MonoBehaviour
{
    public GameObject body;
    public GameObject target;
    public float distMargin = 2;
    public float distSpeed = 3;
    public float rotSpeed = .1f;

    float _rot = 0;
    float _dist;
    float _inner;
    float _outer;

    float _mt = 0;
    float _mdir = 0;
    
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
        _inner = GetComponent<LazerSpawn>().innerCap.transform.localPosition.y;
        _outer = GetComponent<LazerSpawn>().outerCap.transform.localPosition.y;
        _dist = (_inner + _outer) / 2;
    }

    
    void Update()
    {
        var ppos = target.transform.position;
        float playerAngle = Mathf.Atan2(-ppos.x, ppos.y);
        float pmag = ppos.magnitude;

        float angleDiff = UtilFuncs.SmallestAngleDist(_rot, playerAngle);

        _rot += rotSpeed * ((angleDiff > 0) ? 1 : -1) * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * _rot);

        if (_mt < 0)
        {
            if (pmag > _dist + distMargin)
            {
                _mt = 1;
                _mdir = +1;
            } else if (pmag < _dist - distMargin)
            {
                _mt = 1;
                _mdir = -1;
            } else
            {
                _mdir = 0;
            }
        } else
        {
            _mt -= Time.deltaTime;
            _dist += distSpeed * Time.deltaTime * _mdir;
            _dist = Mathf.Clamp(_dist, _inner + 1, _outer - 1);
            body.transform.localPosition = new Vector3(0,_dist,0);
        }

    }
}
