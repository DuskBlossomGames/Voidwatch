using UnityEngine;

namespace Player
{
    public class Movement : MonoBehaviour
    {
        public float driftCorrection;
        public float speedLimit;
        public float acceleration;
    
        private Rigidbody2D _rigid;
        private Camera _camera;
    
        private Vector2 _velocity;
        private float _acceleration;

        private Vector2 _forwards;

        private void Start()
        {
            _camera = Camera.main;
            _rigid = gameObject.GetComponent<Rigidbody2D>();
        }
    
        private void Update()
        {
            if (Input.GetKey("w")) {
                var dv = speedLimit * speedLimit / 100;
                var eff = 1 / (1 + Mathf.Exp(_velocity.sqrMagnitude / 100 - dv));
                if (_velocity.sqrMagnitude > (speedLimit + 5) * (speedLimit + 5)){
                    _acceleration = 0;
                } else {
                    _acceleration = 10 * acceleration * eff;
                }
            
                var vm = _velocity.magnitude;
                _velocity += driftCorrection * Time.deltaTime * Push(_velocity, _forwards);
                _velocity *= (.01f + vm) / (.01f+_velocity.magnitude);

            } else if (Input.GetKey("s")) {
                _velocity *= Mathf.Pow(.2f, Time.deltaTime);
            } else {
                _acceleration = 0;
            }


            var tar = _camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            _forwards = tar.normalized;

            var curAngles = transform.rotation.eulerAngles;
            transform.rotation=Quaternion.Euler(curAngles.x, curAngles.y, -90+Mathf.Rad2Deg*Mathf.Atan2(tar.y, tar.x));
            //forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.rotation), Mathf.Cos(Mathf.Deg2Rad * rigid.rotation));

            _velocity += _forwards * (_acceleration * Time.deltaTime);
            _velocity *= Mathf.Pow(.99f, Time.deltaTime);

            _rigid.velocity = _velocity;
        }
    
        private static Vector2 Push(Vector2 target, Vector2 line)
        {
            var length = target.magnitude;
            var prod = (line.normalized.x * target.x + line.normalized.y * target.y);
            var kv = (prod + length) / 2; //Gives nicer turns, but less snapped in backwards feel
            //float kv = prod; //more control going backwards, but kinda confusing
            var proj = kv * line.normalized;

            var mid = (target.normalized + line.normalized).normalized * length/2;

            return proj - target + mid;
        }
    }
}
