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

    public Form form;
    public float dmg;
    public float segLength;
    public GameObject prev = null;
    public GameObject next = null;
    public GameObject target;
    public float speed = 15;

    private Vector2 _dir;

    (float, float) ToPolar(Vector2 vec)
    {
        float angle = Mathf.Atan2(vec.y, vec.x);
        float radius = vec.magnitude;
        return (angle, radius);
    }
    float MinUnsigned(float x, float y)
    {
        if(Mathf.Abs(x)< Mathf.Abs(y))
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

    private void Start()
    {
        if (target == null) target = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if(form == Form.Head)
        {
            var (tarAng, tarRad) = ToPolar(target.transform.position);
            var (selfAng, selfRad) = ToPolar(transform.position);
            float angDif = SmallestAngleDist(selfAng, tarAng);
            float radDif = tarRad-selfRad;
            float delta = .01f;
            _dir = ((delta * radDif + selfRad) * new Vector2(Mathf.Cos(delta * angDif + selfAng), Mathf.Sin(delta * angDif + selfAng)) 
                - (Vector2)transform.position).normalized; //tiny step along the path in polar coords, which we then form a secant from

            float angle = Mathf.Atan2(_dir.y, _dir.x);
            transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * angle);
            transform.position += speed * (Vector3)_dir * Time.deltaTime;
        }
        if (form == Form.Middle || form == Form.Tail) {
            Vector2 currToPrev = (transform.position - prev.transform.position).normalized;
            transform.position = prev.transform.position + (Vector3) currToPrev * segLength;

            if (form == Form.Middle)
            {
                Vector2 nextToCurr = (next.transform.position - transform.position).normalized;
                Vector2 avg = (nextToCurr + currToPrev) / 2;
                float angle = Mathf.Atan2(avg.y, avg.x);
                transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * angle);

            } else if (form == Form.Tail)
            {
                float angle = Mathf.Atan2(currToPrev.y, currToPrev.x);
                transform.rotation = Quaternion.Euler(0, 0, 90 + Mathf.Rad2Deg * angle);
            }
        }

    }
}
