using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Player
{
    public class Movement : MonoBehaviour
    {
        public float driftCorrection;
        public float speedLimit;
        public float acceleration;
        public float dodgeVelocity;
        public float dodgeDistance;
        public float dodgeCooldown;
        public float dodgeTimeDilation;
        public float afterImageSpacing;

        private Collider2D _collider;
        private SpriteRenderer _sprite;
        private TrailRenderer[] _trails;
        private CustomRigidbody2D _rigid;
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

        private float _lastDodge;
        private float _dodgeTimer;
        private float _afterImageTimer;
        private Vector2 _dodgeDirection;
        private readonly List<GameObject> _afterImages = new();
        
        private void Start()
        {
            _camera = Camera.main;
            _rigid = GetComponent<CustomRigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _trails = GetComponentsInChildren<TrailRenderer>();
        }
    
        private void FixedUpdate()
        {
            var tar = _camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            _forwards = ((Vector2) tar).normalized;
            var curAngles = transform.rotation.eulerAngles;
            transform.rotation=Quaternion.Euler(curAngles.x, curAngles.y, -90+Mathf.Rad2Deg*Mathf.Atan2(tar.y, tar.x));

            if (Time.time - _lastDodge >= dodgeCooldown && _dodgeTimer == 0 && Input.GetKey(KeyCode.Space))
            {
                _dodgeTimer = 1;
                _afterImageTimer = 0;
                _dodgeDirection = new Vector2(_forwards.x, _forwards.y);
            }

            var wasDashing = _dodgeTimer > 0;
            var dashing = (_dodgeTimer = Mathf.Clamp01(_dodgeTimer - Time.fixedDeltaTime * dodgeTimeDilation * dodgeVelocity/dodgeDistance)) > 0;
            CustomRigidbody2D.Scaling = dashing ? dodgeTimeDilation : 1;
            _collider.enabled = !dashing;
            foreach (var trail in _trails) trail.emitting = !dashing;
            _sprite.color = dashing ? new Color(1, 1, 1, 0.5f) : Color.white;
            
            if (dashing)
            {
                if ((_afterImageTimer =
                        Mathf.Clamp01(_afterImageTimer - Time.fixedDeltaTime * dodgeTimeDilation * dodgeVelocity/afterImageSpacing)) == 0)
                {
                    _afterImageTimer = 1;
                    
                    var afterImage = new GameObject
                    {
                        transform =
                        {
                            position = transform.position,
                            rotation = transform.rotation
                        }
                    };
                    
                    var sprite = afterImage.AddComponent<SpriteRenderer>();
                    sprite.sprite = _sprite.sprite;
                    sprite.color = new Color(1, 1, 1, 0.225f);

                    _afterImages.Add(afterImage);
                }
                _rigid.velocity = _dodgeDirection * dodgeVelocity;
                return;
            }
            if (wasDashing)
            {
                _afterImages.ForEach(Destroy);
                _afterImages.Clear();
                
                _velocity = _forwards * _velocity.magnitude;
                _lastDodge = Time.time;
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
            
            //forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.freezeRotation), Mathf.Cos(Mathf.Deg2Rad * rigid.freezeRotation));

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
