using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    Vector2 velocity;
    public Rigidbody2D rigid;
    public float Acceleration;
    public float Rot_Acceleration;
    float accel;
    float rot_acc;
    float rot_vel;
    Vector2 forwards;


    Vector2 project(Vector2 target, Vector2 line)
    {
        Vector2 line_norm = line.normalized;
        return (line_norm.x * target.x + line_norm.y * target.y) * line_norm;
    }

    void Start()
    {
        velocity = new Vector2();
    }

    void Update()
    {
        if (Input.GetKey("w")) {
            accel = 10 * Acceleration;
        } else if (Input.GetKey("s")) {
            velocity *= Mathf.Pow(.2f, Time.deltaTime);
        } else {
            accel = 0;
        }

        if (Input.GetKey("a"))
        {
            rot_acc = 500*Rot_Acceleration;
        } else if (Input.GetKey("d"))
        {
            rot_acc = -500*Rot_Acceleration;
        } else {
            rot_acc = 0;
        }

        forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.rotation), Mathf.Cos(Mathf.Deg2Rad * rigid.rotation));

        velocity += accel * forwards * Time.deltaTime;
        velocity *= Mathf.Pow(.9f, Time.deltaTime);
        rot_vel += rot_acc * Time.deltaTime;
        rot_vel *= Mathf.Pow(.5f, Time.deltaTime);

        rigid.angularVelocity = rot_vel;
        velocity = project(velocity, forwards);
        rigid.velocity = velocity;

    }
}
