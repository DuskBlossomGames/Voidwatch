using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    Vector2 velocity;
    public float driftCorrection;
    public float speedLimit;
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

    Vector2 push(Vector2 target, Vector2 line)
    {
        float length = target.magnitude;
        float prod = (line.normalized.x * target.x + line.normalized.y * target.y);
        float kv = (prod + length) / 2; //Gives nicer turns, but less snapped in backwards feel
        //float kv = prod; //more control going backwards, but kinda confusing
        Vector2 proj = kv * line.normalized;
        return proj - target;
    }

    void Start()
    {
        velocity = new Vector2();
    }

    void Update()
    {
        if (Input.GetKey("w")) {
            float dv = speedLimit * speedLimit / 100;
            float eff = 1 / (1 + Mathf.Exp(velocity.sqrMagnitude / 100 - dv));
            accel = 10 * Acceleration * eff;
            
            float vm = velocity.magnitude;
            velocity += driftCorrection * Time.deltaTime * push(velocity, forwards);
            velocity *= (.01f + vm) / (.01f+velocity.magnitude);

        } else if (Input.GetKey("s")) {
            velocity *= Mathf.Pow(.2f, Time.deltaTime);
        } else {
            accel = 0;
        }


        Vector3 tar = Input.mousePosition - new Vector3(Screen.width/2, Screen.height/2, 0);
        forwards = tar.normalized;
        transform.rotation=Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, -90+Mathf.Rad2Deg*Mathf.Atan2(tar.y, tar.x));
        //forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.rotation), Mathf.Cos(Mathf.Deg2Rad * rigid.rotation));

        velocity += accel * forwards * Time.deltaTime;
        velocity *= Mathf.Pow(.9f, Time.deltaTime);

        rigid.velocity = velocity;

    }
}
