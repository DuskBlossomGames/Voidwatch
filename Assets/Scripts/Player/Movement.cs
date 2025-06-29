using System;
using System.Collections.Generic;
using Spawnables.Controllers.Misslers;
using Static_Info;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;
using static Static_Info.PlayerData;
using static Static_Info.Statistics;
using ProgressBar = ProgressBars.ProgressBar;

namespace Player
{
    public class Movement : MonoBehaviour
    {
        public GameObject explosion;

        public bool inputBlocked;
        public bool autoPilot;

        public ProgressBar dodgeBar;
        public LayerMask dodgeExcludeMask;
        public float dodgeTimeDilation;
        public AnimationCurve dodgeTimeDilationCurve;
        public float afterImageSpacing;
        public Sprite afterImageSprite;

        public GameObject stunnedMsg;
        public float stunMsgRadius;
        public float stunRotPerSec; // degrees
        public float stunBreakStrength;
        public float stunCurveStrength;


        public bool Dodging => !_dodgeTimer.IsFinished;
        public float DodgeJuice => _dodgeJuice;

        [NonSerialized] public Vector2? DodgeOnceDir = null; // auto dodges in this direction
        [NonSerialized] public float? DodgeOnceCost = null;  // auto dodges for this cost (either in DodgeOnceDir or mouse dir)

        private PlayerGunHandler _gun;
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
        private float _dodgeCost;
        private readonly Timer _dodgeTimer = new();
        private readonly Timer _dodgeCostTimer = new();
        private readonly Timer _dodgeCooldownTimer = new();
        private readonly Timer _afterImageTimer = new();
        private Vector2 _dodgeDirection;
        private readonly List<GameObject> _afterImages = new();

        private void Start()
        {
            _gun = GetComponent<PlayerGunHandler>();
            _dodgeJuice = PlayerDataInstance.maxDodgeJuice;
            _camera = Camera.main;
            _rigid = GetComponent<CustomRigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _trails = GetComponentsInChildren<TrailRenderer>();
        }

        public void SetInputBlocked(bool value)
        {
            inputBlocked = value;
            (_gun ?? GetComponent<PlayerGunHandler>()).Shootable = !value;
        }

        private bool GetKey(KeyCode code)
        {
            return !inputBlocked && !autoPilot && InputManager.GetKey(code);
        }
        private bool GetKeyDown(KeyCode code)
        {
            return !inputBlocked && !autoPilot && InputManager.GetKeyDown(code);
        }

        public void DrainDodgeJuice(float amt)
        {
            _dodgeJuice = Mathf.Max(_dodgeJuice-amt, 0);
        }

        private readonly Timer _stunTimer = new();
        public bool Stunned => !_stunTimer.IsFinished;
        public void Stun(float time)
        {
            if (Stunned) return;

            _stunTimer.Value = time;
            SetInputBlocked(true);
            Instantiate(stunnedMsg,
                stunnedMsg.transform.position + stunMsgRadius *
                (Vector3)UtilFuncs.AngleToVector(_camera.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2),
                Camera.main!.transform.rotation).SetActive(true);
            // TODO: stunned VFX
            gameObject.GetComponent<PlayerVFXController>().RunStun();

        }

        private void Update()
        {
            if (!_stealthKeyUp && !_dodgeTimer.IsFinished && InputManager.GetKeyUp(InputAction.Dash)) _stealthKeyUp = true;

            if (_stealthKeyUp && !_redirectDodge && GetKeyDown(InputAction.Dash))
            {
                _redirectDodge = true;
                _redirectDirection = new Vector2(_forwards.x, _forwards.y);
            }
            
            dodgeBar.UpdatePercentage(_dodgeJuice, PlayerDataInstance.maxDodgeJuice); // just always keep up to date
        }

        private void FixedUpdate()
        {
            var velocity = _rigid.linearVelocity;

            var tar = _camera.ScreenToWorldPoint(InputManager.mousePosition) - transform.position;
            _forwards = ((Vector2) tar).normalized;
            var curAngles = transform.rotation.eulerAngles;

            if (!Stunned)
            {
                // look at mouse
                transform.rotation = Quaternion.Euler(curAngles.x, curAngles.y, -90+Mathf.Rad2Deg*Mathf.Atan2(tar.y, tar.x));
            }
            else
            {
                transform.rotation *= Quaternion.Euler(0, 0, stunRotPerSec * Time.fixedDeltaTime);
                velocity *= Mathf.Pow(1-stunBreakStrength, Time.fixedDeltaTime);
                velocity = Quaternion.Euler(0, 0, stunCurveStrength
                                                  * Vector2.Dot(velocity.normalized,
                                                      Quaternion.Euler(0, 0, 90) * transform.position.normalized)
                                                  * Time.fixedDeltaTime) * velocity;

                _stunTimer.FixedUpdate();
                if (!Stunned) SetInputBlocked(false);
            }

            if (_dodgeJuice >= PlayerDataInstance.dodgeJuiceCost && _dodgeCooldownTimer.IsFinished && (GetKey(InputAction.Dash) || DodgeOnceDir != null || DodgeOnceCost != null))
            {
                _preDodgeVel = velocity;
                _dodgeTimeLength = PlayerDataInstance.dodgeDistance / PlayerDataInstance.dodgeVelocity;
                _dodgeTimer.Value = _dodgeTimeLength;
                _dodgeCostTimer.Value = _dodgeTimeLength;
                _dodgeCost = DodgeOnceCost ?? PlayerDataInstance.dodgeJuiceCost;
                _dodgeCooldownTimer.Value = PlayerDataInstance.dodgeDistance / PlayerDataInstance.dodgeVelocity + PlayerDataInstance.dodgeCooldown;
                _dodgeDirection = DodgeOnceDir ?? new Vector2(_forwards.x, _forwards.y);
                _gun.HasDodgePowerAttack = true;

                if (PlayerDataInstance.dodgeExplosionDamage > 0)
                {
                    var obj = Instantiate(explosion);
                    obj.transform.position = transform.position;
                    obj.GetComponent<ExplosionHandler>().Run(PlayerDataInstance.dodgeExplosionDamage,
                        1.5f + PlayerDataInstance.dodgeExplosionDamage / 25, gameObject,
                        new List<Collider2D> { _collider });
                }

                StatisticsInstance.timesDashed++;

                DodgeOnceDir = null;
                DodgeOnceCost = null;
            }

            var wasDodging = !_dodgeTimer.IsFinished;
            _dodgeTimer.FixedUpdate();
            _dodgeCostTimer.FixedUpdate();
            _dodgeCooldownTimer.FixedUpdate();
            _afterImageTimer.FixedUpdate();
            var dodging = !_dodgeTimer.IsFinished;

            if (1-_dodgeTimer.Progress >= PlayerDataInstance.dodgeRedirectPercentage && !_redirected && _redirectDodge)
            {
                _redirected = true;
                _dodgeTimer.SetValue(_dodgeTimeLength / 2);
                _dodgeDirection = _redirectDirection;
            }


            // CustomRigidbody2D.Scaling = dodging ? dodgeTimeDilationCurve.Evaluate(1 - _dodgeTimer.Value/_dodgeTimeLength) : 1;
            CustomRigidbody2D.Scaling = dodging ? dodgeTimeDilation : 1;
            _collider.excludeLayers = dodging ? dodgeExcludeMask : 0;
            _collider.layerOverridePriority = dodging ? 1000 : 1;
            foreach (var trail in _trails) trail.emitting = !dodging;
            _sprite.color = dodging ? new Color(1, 1, 1, 0.5f) : Color.white;
            
            _dodgeJuice = Mathf.Clamp(_dodgeJuice + (!dodging ? PlayerDataInstance.dodgeJuiceRegenRate
                : !_dodgeCostTimer.IsFinished ? -_dodgeCost/_dodgeTimeLength*dodgeTimeDilation
                : 0) * Time.fixedDeltaTime, 0, PlayerDataInstance.maxDodgeJuice);
            dodgeBar.UpdatePercentage(_dodgeJuice, PlayerDataInstance.maxDodgeJuice);

            if (dodging)
            {
                if (_afterImageTimer.IsFinished)
                {
                    _afterImageTimer.Value = afterImageSpacing / PlayerDataInstance.dodgeVelocity;

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
                _rigid.linearVelocity = _dodgeDirection * PlayerDataInstance.dodgeVelocity;
                return;
            }
            if (wasDodging)
            {
                _afterImages.ForEach(Destroy);
                _afterImages.Clear();
                _redirectDodge = _redirected = _stealthKeyUp = false;

                velocity = _forwards * _preDodgeVel.magnitude;
            }

            if (GetKey(InputAction.Accelerate))
            {
                var dv = PlayerDataInstance.speedLimit * PlayerDataInstance.speedLimit / 100;
                var eff = 1 / (1 + Mathf.Exp(velocity.sqrMagnitude / 100 - dv));
                if (velocity.sqrMagnitude > (PlayerDataInstance.speedLimit + 5) * (PlayerDataInstance.speedLimit + 5)){
                    _acceleration = 0;
                } else {
                    _acceleration = 10 * PlayerDataInstance.acceleration * eff;
                }

                var vm = velocity.magnitude;
                velocity += PlayerDataInstance.driftCorrection * Time.fixedDeltaTime * Push(velocity, _forwards);
                velocity *= (.01f + vm) / (.01f+velocity.magnitude);

                /*if (GetKey("a"))
                {
                    _velocity = _or
                }*/

            } else if (GetKey(InputAction.Brake) || autoPilot) {
                velocity *= Mathf.Pow(.2f, Time.fixedDeltaTime);
            } else {
                _acceleration = 0;
            }

            if (autoPilot && ((Vector2)transform.position).sqrMagnitude > 60 * 60) velocity -= (Vector2)transform.position * Time.fixedDeltaTime;

            if (velocity.sqrMagnitude > PlayerDataInstance.speedLimit * PlayerDataInstance.speedLimit)
            {
                velocity *= Mathf.Pow(PlayerDataInstance.speedLimit * PlayerDataInstance.speedLimit / velocity.sqrMagnitude, Time.fixedDeltaTime);
            }
            //forwards = new Vector2(-Mathf.Sin(Mathf.Deg2Rad * rigid.freezeRotation), Mathf.Cos(Mathf.Deg2Rad * rigid.freezeRotation));

            if (!autoPilot) velocity += _forwards * (_acceleration * Time.fixedDeltaTime);
            velocity *= Mathf.Pow(.99f, Time.fixedDeltaTime);
            StatisticsInstance.distanceTraveled += velocity.magnitude * Time.fixedDeltaTime;

            _rigid.linearVelocity = velocity;
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
