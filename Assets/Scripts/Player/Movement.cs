using System;
using System.Collections.Generic;
using ProgressBars;
using Scriptable_Objects.Upgrades;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using Random = UnityEngine.Random;
using static Static_Info.PlayerData;

namespace Player
{
    public class Movement : MonoBehaviour
    {
        public bool inputBlocked;
        public bool autoPilot;

        public float driftCorrection;
        public float speedLimit;
        public float acceleration;
        public ProgressBar dodgeBar;
        public float dodgeRedirectPercentage;
        public float dodgeJuiceCost;
        public float dodgeVelocity;
        public float dodgeDistance;
        public float dodgeCooldown;
        public float dodgeTimeDilation;
        public AnimationCurve dodgeTimeDilationCurve;
        public float afterImageSpacing;
        public Sprite afterImageSprite;

        public bool Dodging => !_dodgeTimer.IsFinished;
        
        private Collider2D _collider;
        private SpriteRenderer _sprite;
        private TrailRenderer[] _trails;
        private CustomRigidbody2D _rigid;
        private Camera _camera;

        private Vector2 _preDodgeVel;
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
        
        private bool _redirectDodge;
        private bool _redirected;
        private bool _stealthKeyUp;
        private Vector2 _redirectDirection;
        private float _dodgeTimeLength;
        private float _dodgeJuice;
        private readonly Timer _dodgeTimer = new();
        private readonly Timer _dodgeCooldownTimer = new();
        private readonly Timer _afterImageTimer = new();
        private Vector2 _dodgeDirection;
        private readonly List<GameObject> _afterImages = new();

        private Upgradeable _upgradeable;

        private void Start()
        {
            _dodgeJuice = PlayerDataInstance.maxDodgeJuice;
            _camera = Camera.main;
            _rigid = GetComponent<CustomRigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _trails = GetComponentsInChildren<TrailRenderer>();
            _upgradeable = GetComponent<Upgradeable>();
        }

        private bool GetKey(KeyCode code)
        {
            return !inputBlocked && Input.GetKey(code);
        }
        private bool GetKeyDown(KeyCode code)
        {
            return !inputBlocked && Input.GetKeyDown(code);
        }

        private void Update()
        {
            if (!_stealthKeyUp && !_dodgeTimer.IsFinished && Input.GetKeyUp(KeyCode.Space)) _stealthKeyUp = true;
            
            if (_stealthKeyUp && !_redirectDodge && GetKeyDown(KeyCode.Space))
            {
                _redirectDodge = true;
                _redirectDirection = new Vector2(_forwards.x, _forwards.y);
            }
        }

        private void FixedUpdate()
        {
            var velocity = _rigid.velocity;

            var tar = _camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            _forwards = ((Vector2) tar).normalized;
            var curAngles = transform.rotation.eulerAngles;
            transform.rotation=Quaternion.Euler(curAngles.x, curAngles.y, -90+Mathf.Rad2Deg*Mathf.Atan2(tar.y, tar.x));
            
            if (_dodgeJuice >= dodgeJuiceCost && _dodgeCooldownTimer.IsFinished && GetKey(KeyCode.Space))
            {
                var evt = new DodgeEvent
                {
                    dodgeCooldown = dodgeCooldown,
                    dodgeDistance = dodgeDistance,
                    dodgeVelocity = dodgeVelocity
                };
                if (_upgradeable) _upgradeable.HandleEvent(evt, null);

                _preDodgeVel = velocity;
                _dodgeTimeLength = evt.dodgeDistance / evt.dodgeVelocity;
                _dodgeTimer.Value = _dodgeTimeLength;
                _dodgeCooldownTimer.Value = evt.dodgeDistance / evt.dodgeVelocity + evt.dodgeCooldown;
                _dodgeDirection = new Vector2(_forwards.x, _forwards.y);
            }

            var wasDodging = !_dodgeTimer.IsFinished;
            _dodgeTimer.FixedUpdate();
            _dodgeCooldownTimer.FixedUpdate();
            _afterImageTimer.FixedUpdate();
            var dodging = !_dodgeTimer.IsFinished;

            if (1-_dodgeTimer.Progress >= dodgeRedirectPercentage && !_redirected && _redirectDodge)
            {
                _redirected = true;
                _dodgeTimer.SetValue(_dodgeTimeLength / 2);
                _dodgeDirection = _redirectDirection;
            } 


            // CustomRigidbody2D.Scaling = dodging ? dodgeTimeDilationCurve.Evaluate(1 - _dodgeTimer.Value/_dodgeTimeLength) : 1;
            CustomRigidbody2D.Scaling = dodging ? dodgeTimeDilation : 1;
            _collider.enabled = !dodging;
            foreach (var trail in _trails) trail.emitting = !dodging;
            _sprite.color = dodging ? new Color(1, 1, 1, 0.5f) : Color.white;

            _dodgeJuice = Mathf.Clamp(_dodgeJuice + (!dodging ? PlayerDataInstance.dodgeJuiceRegenRate : -dodgeJuiceCost/_dodgeTimeLength*dodgeTimeDilation) * Time.fixedDeltaTime, 0, PlayerDataInstance.maxDodgeJuice);
            dodgeBar.UpdatePercentage(_dodgeJuice, PlayerDataInstance.maxDodgeJuice);
            
            if (dodging)
            {
                if (_afterImageTimer.IsFinished)
                {
                    _afterImageTimer.Value = afterImageSpacing / dodgeVelocity;

                    var afterImage = new GameObject
                    {
                        transform =
                        {
                            position = transform.position,
                            rotation = transform.rotation
                        }
                    };

                    var sprite = afterImage.AddComponent<SpriteRenderer>();
                    sprite.sprite = afterImageSprite;
                    Color tempColor = Color.HSVToRGB(Random.Range(0.75f,0.85f), 0.5f, 0.5f);
                    tempColor.a = 0.5f;
                    sprite.color = tempColor;

                    _afterImages.Add(afterImage);
                }
                _rigid.velocity = _dodgeDirection * dodgeVelocity;
                return;
            }
            if (wasDodging)
            {
                _afterImages.ForEach(Destroy);
                _afterImages.Clear();
                _redirectDodge = _redirected = _stealthKeyUp = false;

                velocity = _forwards * _preDodgeVel.magnitude;
            }

            if (GetKey(KeyCode.W))
            {
                var evt = new MoveEvent { speedLimit = speedLimit, acceleration = acceleration };
                if (_upgradeable) _upgradeable.HandleEvent(evt, null);

                var dv = evt.speedLimit * evt.speedLimit / 100;
                var eff = 1 / (1 + Mathf.Exp(velocity.sqrMagnitude / 100 - dv));
                if (velocity.sqrMagnitude > (evt.speedLimit + 5) * (evt.speedLimit + 5)){
                    _acceleration = 0;
                } else {
                    _acceleration = 10 * evt.acceleration * eff;
                }

                var vm = velocity.magnitude;
                velocity += driftCorrection * Time.fixedDeltaTime * Push(velocity, _forwards);
                velocity *= (.01f + vm) / (.01f+velocity.magnitude);

                /*if (GetKey("a"))
                {
                    _velocity = _or
                }*/

            } else if (GetKey(KeyCode.S) || autoPilot) {
                velocity *= Mathf.Pow(.2f, Time.fixedDeltaTime);
            } else {
                _acceleration = 0;
            }

            if (autoPilot && ((Vector2)transform.position).sqrMagnitude > 60 * 60) velocity -= (Vector2)transform.position * (2 * Time.fixedDeltaTime);

            if (velocity.sqrMagnitude > speedLimit * speedLimit)
            {
                velocity *= Mathf.Pow(speedLimit * speedLimit / velocity.sqrMagnitude, Time.fixedDeltaTime);
            }
            //forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.freezeRotation), Mathf.Cos(Mathf.Deg2Rad * rigid.freezeRotation));

            velocity += _forwards * (_acceleration * Time.fixedDeltaTime);
            velocity *= Mathf.Pow(.99f, Time.fixedDeltaTime);

            _rigid.velocity = velocity;
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
