using UnityEngine;
using Util;

public class MissilerPathfinding : MonoBehaviour
{
    public GameObject target;
    public float minDist;
    public float speed = 10;


    private Vector3 _tar;
    private bool _moving = false;
    private CustomRigidbody2D _rigid;


    void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        _rigid = GetComponent<CustomRigidbody2D>();
    }


    void Update()
    {
        if (!_moving) {
            if ((transform.position - target.transform.position).sqrMagnitude > minDist * minDist)
            {
                float mag = target.transform.position.magnitude;
                _tar = mag * (transform.position + target.transform.position).normalized;
                _moving = true;
            }
        } else
        {
            Vector3 dif = _tar - transform.position;
            if (dif.sqrMagnitude < 10)
            {
                _moving = false;
            }
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dif.y,dif.x));
            _rigid.AddRelativeForce(new Vector2(speed, 0));
            _rigid.velocity = Vector2.ClampMagnitude(_rigid.velocity, speed);
        }
    }
}
