using UnityEngine;
using Util;

namespace Spawnables.Controllers.Misslers
{
    public class MissilerPathfinding : MonoBehaviour
    {
        public GameObject target;
        public float moveAwayDist, moveTowardsDist;
        public float speed = 10;


        private Vector3 _tar;
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

            var rot = Mathf.LerpAngle(transform.rotation.eulerAngles.z, Mathf.Rad2Deg * Mathf.Atan2(dif.y, dif.x), 10 * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, rot);
            _rigid.AddRelativeForce(new Vector2(Mathf.Abs(mult)*speed, 0));
            _rigid.linearVelocity = Vector2.ClampMagnitude(_rigid.linearVelocity, speed);
            if (mult == 0) _rigid.linearVelocity *= Mathf.Pow(0.3f, Time.deltaTime);

            if (((Vector2)transform.position).sqrMagnitude > 75 * 75)
            {
                _rigid.AddForce(Quaternion.Euler(0, 0, 45) * ((Vector2)transform.position).normalized * -20);
            }
        }
    }
}
