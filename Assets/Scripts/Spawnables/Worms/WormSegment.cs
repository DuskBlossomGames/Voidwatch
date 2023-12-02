using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormSegment : MonoBehaviour
{
    public enum Form
    {
        Head,
        Middle,
        Tail
    };

    public enum PathMode
    {
        Around,
        Direct
    };

    public PathMode pathmode;
    public Form form;
    public float dmg;
    public float segLength;
    public GameObject prev = null;
    public GameObject next = null;
    public GameObject target;
    public float speed = 15;
    public float accel;
    public AnimationCurve accelCurve;
    public float snakeyness;
    public float snaketime;
    public float atkRange;
    public float chargeUpTime;

    private Vector2 _dir;
    private float _currSpeed;
    private float _currSnakeTime;
    private float _chargeUp;
    public float bend;

    public Vector2 expDir;

    private void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if(form == Form.Head)
        {
            _currSnakeTime = (_currSnakeTime + Time.deltaTime)%snaketime;

            if (pathmode == PathMode.Around)
            {
                var (tarAng, tarRad) = ToPolar(target.transform.position);
                var (selfAng, selfRad) = ToPolar(transform.position);
                float angDif = SmallestAngleDist(selfAng, tarAng);
                float radDif = tarRad - selfRad;
                float delta = .01f;
                _dir = ((delta * radDif + selfRad) * new Vector2(Mathf.Cos(delta * angDif + selfAng), Mathf.Sin(delta * angDif + selfAng))
                    - (Vector2)transform.position).normalized; //tiny step along the path in polar coords, which we then form a secant from
            } else if(pathmode ==PathMode.Direct)
            {
                _dir = (target.transform.position - transform.position).normalized;
            }

            float angle = Mathf.Atan2(_dir.y, _dir.x);
            float snakeAngle = snakeyness * Mathf.Sin(2 * Mathf.PI * _currSnakeTime / snaketime);
            angle += snakeAngle;
            expDir = _dir;
            _dir = rot(_dir, snakeAngle);

            transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * angle);
            
            var rigid = GetComponent<Rigidbody2D>();
            float deltVel = accel * accelCurve.Evaluate(rigid.velocity.magnitude/speed) * Time.deltaTime;
            _currSpeed += deltVel;
            _currSpeed = Mathf.Min(_currSpeed, speed);
            rigid.velocity = _currSpeed * _dir;


            if((transform.position - target.transform.position).sqrMagnitude < atkRange * atkRange)
            {
                _chargeUp += Time.deltaTime;
                if (_chargeUp > chargeUpTime)
                {
                    _chargeUp = 0;
                    GetComponent<WormBeamAttack>().Shoot();
                }

            } else
            {
                _chargeUp = 0;
            }

        }
        if (form == Form.Middle || form == Form.Tail) {
            Vector2 currToPrev = (transform.position - prev.transform.position).normalized;
            transform.position = prev.transform.position + (Vector3) currToPrev * segLength;

            if (form == Form.Middle)
            {
                Vector2 nextToCurr = (next.transform.position - transform.position).normalized;
                Vector2 avg = (nextToCurr + currToPrev) / 2;
                float angle = Mathf.Atan2(avg.y, avg.x);
                bend = Vector2.Dot(nextToCurr, currToPrev);
                transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * angle);

            } else if (form == Form.Tail)
            {
                float angle = Mathf.Atan2(currToPrev.y, currToPrev.x);
                transform.rotation = Quaternion.Euler(0, 0, 90 + Mathf.Rad2Deg * angle);
            }
        }

    }

    public Vector2 PredDir(float time)
    {
        return (target.transform.position + time * (Vector3)target.GetComponent<Rigidbody2D>().velocity - transform.position).normalized;
    }
    Vector2 rot(Vector2 vec, float angle)
    {
        return new Vector2(vec.x * Mathf.Cos(angle) - vec.y * Mathf.Sin(angle), vec.x * Mathf.Sin(angle) + vec.y * Mathf.Cos(angle));
    }

    (float, float) ToPolar(Vector2 vec)
    {
        float angle = Mathf.Atan2(vec.y, vec.x);
        float radius = vec.magnitude;
        return (angle, radius);
    }
    float MinUnsigned(float x, float y)
    {
        if (Mathf.Abs(x) < Mathf.Abs(y))
        {
            return x;
        }
        else
        {
            return y;
        }
    }

    float SmallestAngleDist(float orig, float tar)
    {
        return MinUnsigned(MinUnsigned(tar - orig, tar + 2 * Mathf.PI - orig), tar - 2 * Mathf.PI - orig);
    }
}
