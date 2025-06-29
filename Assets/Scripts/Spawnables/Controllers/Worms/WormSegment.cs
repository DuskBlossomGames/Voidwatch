using Spawnables.Pathfinding;
using UnityEngine;
using Util;

namespace Spawnables.Controllers.Worms
{
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
        public SnakePathfinder aroundPather;
        public DirectPathfinding directPather;
        public Form form;
        public float dmg;
        public float segLength;
        public GameObject prev = null;
        public GameObject next = null;
        public GameObject target;
        public float speed = 15;
        public float accel;
        public AnimationCurve accelCurve;
        public float atkRange;
        public float chargeUpTime;

        private Vector2 _dir;
        private float _currSpeed;
        private float _chargeUp;
        public float bend;

        private void Start()
        {
            if (target == null) target = GameObject.FindGameObjectWithTag("Player");
        }

        private void RotateTo(Vector2 dir)
        {
            _dir = dir;
            var angle = Mathf.Atan2(_dir.y, _dir.x);
            transform.rotation = Quaternion.Euler(0, 0, -90 + Mathf.Rad2Deg * angle);
        }

        void Update()
        {
            if(form == Form.Head)
            {
                RotateTo(pathmode == PathMode.Around
                    ? aroundPather.PathDirNorm(transform.position, target.transform.position)
                    : directPather.PathDirNorm(transform.position, target.transform.position));
            
                var rigid = GetComponent<CustomRigidbody2D>();
                float deltVel = accel * accelCurve.Evaluate(rigid.linearVelocity.magnitude/speed) * Time.deltaTime;
                _currSpeed = Mathf.Min(_currSpeed+ deltVel, speed);
                rigid.linearVelocity = _currSpeed * _dir;


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
                if (prev == null) return;
                
                Vector2 currToPrev = (transform.position - prev.transform.position).normalized;
                transform.position = prev.transform.position + (Vector3) currToPrev * segLength;

                if (form == Form.Middle)
                {
                    Vector2 nextToCurr = (next.transform.position - transform.position).normalized;
                    Vector2 avg = -(nextToCurr + currToPrev) / 2;
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

        public Vector2 PredDir(Vector3 myPos, float time)
        {
            return UtilFuncs.PredictTargetPos(target.transform.position - myPos, UtilFuncs.GetTargetVel(target), time).normalized;
        }
    }
}
