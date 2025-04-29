using UnityEngine;
using Util;

public class MissilerPathfinding : MonoBehaviour
{
    public GameObject target;
    public float moveAwayDist, moveTowardsDist;
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
        var mult = 0;

        var dif = (Vector2)(target.transform.position - transform.position);
        
        if (dif.sqrMagnitude > moveTowardsDist * moveTowardsDist) mult = 1;
        else if (dif.sqrMagnitude < moveAwayDist * moveAwayDist) mult = -1;
        
        dif *= mult;

        transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(dif.y,dif.x));
        _rigid.AddRelativeForce(new Vector2(Mathf.Abs(mult)*speed, 0));
        _rigid.velocity = Vector2.ClampMagnitude(_rigid.velocity, speed);
        if (mult == 0) _rigid.velocity *= Mathf.Pow(0.3f, Time.deltaTime);
    }
}
