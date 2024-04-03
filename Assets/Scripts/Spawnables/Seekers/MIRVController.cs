using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MIRVController : MonoBehaviour
{
    MissleAim _ma;
    public Transform target;
    public float splitDist;
    public int splitCount;
    public GameObject splitPrefab;
    public float scatterForce;
    public float prepTime = 1;

    bool _canPop = false;

    Util.Timer _timer;

    Util.CustomRigidbody2D _crb;

    // Start is called before the first frame update
    void Start()
    {
        _ma = GetComponent<MissleAim>();
        if (_ma.target != null) {
            target = _ma.target.transform;
        }
        _crb = GetComponent<Util.CustomRigidbody2D>();
        _timer = new Util.Timer();
    }

    // Update is called once per frame
    void Update()
    {
        while(target == null)
        {
            if (_ma.target != null)
            {
                target = _ma.target.transform;
                _timer.Value = prepTime;
                _canPop = true;
            }
            else
            {
                return;
            }
        }

        _timer.Update();

        if (_canPop && _timer.IsFinished && (transform.position - target.position).sqrMagnitude < splitDist * splitDist)
        {
            for (int i = 0; i < splitCount; i++)
            {
                var nchild = Instantiate(splitPrefab);
                var nma = nchild.GetComponent<MissleAim>();
                nma.target = target.gameObject;
                var ncrb = nchild.GetComponent<Util.CustomRigidbody2D>();
                ncrb.velocity = _crb.velocity;
                ncrb.AddForce(scatterForce * Random.insideUnitCircle);
                nchild.transform.position = transform.position;
            }
            Destroy(gameObject);
        }
    }
}
