using UnityEngine;

namespace Player
{
    public class Movement : MonoBehaviour
    {
        public float driftCorrection;
        public float speedLimit;
        public float acceleration;
        public float dashTime;
        public float dashDistance;
        public float dashTimeDilation;
    
        private Rigidbody2D _rigid;
        private Camera _camera;
    
        private Vector2 _velocity;
        private float _acceleration;

        private Vector2 _forwards;
        private struct OrbitState
        {
            float _orbitDistance;
            bool _isAntiClockwise;
            Vector2 _targetVel;
            Vector2 _orbitPoint;
        };
        private OrbitState _orbitState;

        private float _dashTimer;
        private Vector2 _dashDirection;
        
        private Collider2D _collider;

        private void Start()
        {
            _camera = Camera.main;
            _rigid = gameObject.GetComponent<Rigidbody2D>();
            _collider = gameObject.GetComponent<Collider2D>();
        }
    
        private void FixedUpdate()
        {
            var tar = _camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            _forwards = ((Vector2) tar).normalized;
            var curAngles = transform.rotation.eulerAngles;
            transform.rotation=Quaternion.Euler(curAngles.x, curAngles.y, -90+Mathf.Rad2Deg*Mathf.Atan2(tar.y, tar.x));

            if (_dashTimer == 0 && Input.GetKey(KeyCode.LeftShift))
            {
                _dashTimer = 1;
                _dashDirection = new Vector2(_forwards.x, _forwards.y);
            }

            var wasDashing = _dashTimer > 0;
            var dashing = (_dashTimer = Mathf.Clamp01(_dashTimer - Time.fixedDeltaTime / dashTime)) > 0;
            Time.timeScale = dashing ? dashTimeDilation : 1;
            _collider.enabled = !dashing;
            
            if (dashing)
            {
                _rigid.velocity = _dashDirection * (dashDistance / dashTime / dashTimeDilation);
                return;
            }
            if (wasDashing)
            {
                Debug.Log("dash ended\t" + _forwards + "\t" + _velocity.magnitude);
                _velocity = _forwards * _velocity.magnitude;
            }
            
            if (Input.GetKey("w")) {
                var dv = speedLimit * speedLimit / 100;
                var eff = 1 / (1 + Mathf.Exp(_velocity.sqrMagnitude / 100 - dv));
                if (_velocity.sqrMagnitude > (speedLimit + 5) * (speedLimit + 5)){
                    _acceleration = 0;
                } else {
                    _acceleration = 10 * acceleration * eff;
                }
            
                var vm = _velocity.magnitude;
                _velocity += driftCorrection * Time.fixedDeltaTime * Push(_velocity, _forwards);
                _velocity *= (.01f + vm) / (.01f+_velocity.magnitude);

                /*if (Input.GetKey("a"))
                {
                    _velocity = _or
                }*/

            } else if (Input.GetKey("s")) {
                _velocity *= Mathf.Pow(.2f, Time.fixedDeltaTime);
            } else {
                _acceleration = 0;
            }
            
            //forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.rotation), Mathf.Cos(Mathf.Deg2Rad * rigid.rotation));

            _velocity += _forwards * (_acceleration * Time.fixedDeltaTime);
            _velocity *= Mathf.Pow(.99f, Time.fixedDeltaTime);

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
