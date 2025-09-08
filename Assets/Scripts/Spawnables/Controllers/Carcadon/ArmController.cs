using System.Linq;
using Player;
using Singletons;
using UnityEngine;
using Util;
using static Singletons.Static_Info.PlayerData;

namespace Spawnables.Controllers.Carcadon
{
    public class ArmController : MonoBehaviour
    {
        // all arrays start at the outside (claw) and work in
        public Vector2 clawTip;
        public Vector2[] joints;
        public float[] foldRotations;
        public float timeToFold;
        public bool hasAttack;

        public int minSlash, maxSlash;
        public float slashDelay, minAttackDelay, maxAttackDelay;

        public float timeToOpen, timeToClose, timeToReturn;
        private float _attackProgress;

        private GameObject[] _segments;

        private float _foldProgress;
        private float _foldSpeed;
        private int _foldDirection;

        private bool _attacking;
        private Vector2? _target;

        private Movement _player;
        public GameObject Player
        {
            set
            {
                _player = value.GetComponent<Movement>();
                var cd = GetComponentInChildren<ClawDamage>();
                if (cd != null) cd.Player = value.GetComponent<PlayerDamageable>();
            }
        }

        private bool _missed, _shownMissed;
        
        private ClawDamage _clawDamage;
        private PolygonCollider2D _collider;
        private CircleCollider2D _playerCol;

        private int _curAttackSlashes;
        private int _curSlash;
        private readonly Timer _attackCooldown = new();
        private readonly Timer _slashCooldown = new();

        public AudioClip slashClip;
        private bool _clawSoundBegan;

        private float[] _origJointAngles;
        
        private void Start()
        {
            _collider = GetComponent<PolygonCollider2D>();
            _clawDamage = GetComponentInChildren<ClawDamage>();
            _playerCol = _player.GetComponent<CircleCollider2D>();

            _segments = new [] {transform.GetChild(3).gameObject, transform.GetChild(2).gameObject,
                transform.GetChild(1).gameObject, transform.GetChild(0).gameObject};


            _foldSpeed = foldRotations.Sum(Mathf.Abs) / timeToFold;

            _curAttackSlashes = Random.Range(minSlash, maxSlash);

            _origJointAngles = new float[_segments.Length];
            for (var joint = 0; joint < _segments.Length; joint++) _origJointAngles[joint] = GetJointAngle(joint);
        }

        public void FoldClosed() { _foldDirection = 1; }
        public void FoldOpen() { _foldDirection = -1; }

        public void SetFolded(bool folded)
        {
            if (folded == _folded) return;

            _foldProgress = folded ? _segments.Length : 0;
            _folded = folded;

            if (folded)
            {
                for (var i = 0; i < _segments.Length; i++) RotateJointTo(i, foldRotations[i]);
            }
            else
            {
                for (var i = _segments.Length - 1; i >= 0; i--) RotateJointTo(i, 0);
            }
        }

        public void VisualAttack()
        {
            _clawDamage.transform.parent.GetChild(1).gameObject.SetActive(true); // enable trail renderer

            _curAttackSlashes = 1;
            _curSlash = 0;
            _attackProgress += Time.fixedDeltaTime;
        }

        private float MinOrMax(float a, float b)
        {
            return _foldDirection == 1 ? (a + 1 < b ? a + 1 : b) : (a > b ? a : b);
        }

        private int FloorOrCeil(float a)
        {
            return _foldDirection == 1 ? (int)a : Mathf.CeilToInt(a - 1);
        }

        private bool _folded;
        private void FixedUpdate()
        {
            _attackCooldown.FixedUpdate();
            _slashCooldown.FixedUpdate();

            // fold
            if (_attackProgress == 0 && _foldDirection != 0)
            {
                var currentSeg = Mathf.Clamp(FloorOrCeil(_foldProgress), 0, foldRotations.Length-1);
                var rotation = _foldDirection * Mathf.Sign(foldRotations[currentSeg]) * _foldSpeed * Time.fixedDeltaTime;

                _foldProgress = MinOrMax(currentSeg, _foldProgress + rotation / foldRotations[currentSeg]);

                RotateJointTo(currentSeg, (_foldProgress-currentSeg) * foldRotations[currentSeg]);

                if (_foldProgress <= 0 || _foldProgress >= _segments.Length)
                {
                    SetFolded(_foldDirection == 1); // fail-safe
                    _foldDirection = 0;
                }
            }
            
            if ((hasAttack && _foldDirection == 0 && !_folded && _attackCooldown.IsFinished && _slashCooldown.IsFinished && _collider.OverlapPoint(_player.transform.position)) || _attackProgress != 0)
            {
                if (_missed && !_shownMissed)
                {
                    foreach (var seg in _segments)
                    {
                        var myCol = seg.GetComponentInChildren<BoxCollider2D>();
                        if (!myCol.bounds.Intersects(_playerCol.bounds)) continue;

                        _shownMissed = true;
                        _player.ShowBillboard(BillboardMessage.Missed, (Vector2) _player.transform.position + 0.2f*_player.GetComponent<CustomRigidbody2D>().linearVelocity);
                        break;
                    }
                }
                
                if (!_missed && !_player.Dodging)
                {
                    // avoid player going through them
                    for (var i = 0; i < _segments.Length; i++)
                    {
                        var myCol = _segments[i].GetComponentInChildren<BoxCollider2D>();
                        if (!myCol.bounds.Intersects(_playerCol.bounds)) continue;
                        
                        var arm = (Vector2) _segments[i].transform.position - GetJoint(i);
                        var normalAngle = Mathf.Atan2(arm.y, arm.x) * Mathf.Rad2Deg + 90;

                        var playerDist = (Quaternion.Euler(0, 0, -normalAngle) * (_player.transform.position - _segments[i].transform.position)).x;

                        _player.transform.position += Quaternion.Euler(0, 0, normalAngle) * new Vector3(myCol.transform.lossyScale.x*myCol.size.x/2 + _playerCol.transform.lossyScale.x*_playerCol.radius/2 - playerDist, 0, 0);
                    }
                }

                if (_attackProgress == 0)
                {
                    _missed = Random.value < PlayerDataInstance.missChance;
                    _shownMissed = false;
                    foreach (var seg in _segments)
                    {
                        Physics2D.IgnoreCollision(seg.GetComponentInChildren<BoxCollider2D>(), _playerCol, _missed);
                    }
                    
                    _clawDamage.Active = !_missed;
                    _clawDamage.transform.parent.GetChild(1).gameObject.SetActive(true); // enable trail renderer
                }

                var openBase = (Mathf.Sign(foldRotations[^1]) * -180 + foldRotations[^1]) * 3/4;
                var openNext = Mathf.Sign(foldRotations[^2]) * -180 + foldRotations[^2];

                var foldLast = foldRotations[^3]/2;
                var foldClaw = foldRotations[0] * 3/4;


                if (_attackProgress < timeToOpen)
                {
                    var prog = _attackProgress / timeToOpen;
                    RotateJointTo(_segments.Length - 1,  prog * openBase);
                    RotateJointTo(_segments.Length - 2, prog * openNext);

                    if (!_clawSoundBegan)
                    {
                        AudioPlayer.Play(slashClip, this, Random.Range(0.8f, 1f), 0.7f);
                        _clawSoundBegan = true;
                    }
                    
                } else if (_attackProgress - timeToOpen < timeToClose)
                {
                    var prog = (_attackProgress-timeToOpen) / timeToClose;
                    RotateJointTo(_segments.Length - 2, (1-prog) * openNext);
                    RotateJointTo(_segments.Length - 3, prog * foldLast);
                    RotateJointTo(0, prog * foldClaw);

                } else if (_attackProgress - timeToOpen - timeToClose < timeToReturn)
                {
                    var prog = (_attackProgress - timeToOpen - timeToClose) / timeToReturn;
                    RotateJointTo(_segments.Length - 1, (1-prog) * openBase);
                    RotateJointTo(_segments.Length - 3, (1-prog) * foldLast);
                    RotateJointTo(0, (1-prog) * foldClaw);
                }
                else
                {
                    foreach (var seg in _segments)
                    {
                        Physics2D.IgnoreCollision(seg.GetComponentInChildren<BoxCollider2D>(), _playerCol, false);
                    }

                    SetFolded(false); // fail-safe
                    _clawDamage.Active = false;
                    _clawSoundBegan = false;
                    _clawDamage.transform.parent.GetChild(1).gameObject.SetActive(false); // disable trail renderer
                    _attackProgress = 0;

                    _curSlash += 1;
                    if (_curSlash == _curAttackSlashes)
                    {
                        _attackCooldown.Value = Random.Range(minAttackDelay, maxAttackDelay);
                        _curAttackSlashes = Random.Range(minSlash, maxSlash);
                        _curSlash = 0;
                    }
                    else
                    {
                        _slashCooldown.Value = slashDelay;
                    }

                    return;
                }

                _attackProgress += Time.fixedDeltaTime;
            }
        }

        private Vector2 GetJoint(int i)
        {
            return _segments[i].transform.TransformPoint(joints[i]);
        }

        private float GetJointAngle(int joint)
        {
            var seg1 = (joint == 3 ? (Vector2)(transform.rotation * Vector2.up) : GetJoint(joint + 1) - GetJoint(joint)).normalized;
            var seg2 = ((joint == 0 ? _segments[0].transform.TransformPoint(clawTip) : GetJoint(joint-1)) - GetJoint(joint)).normalized;
            
            return Mathf.DeltaAngle(Mathf.Atan2(seg1.y, seg1.x) * Mathf.Rad2Deg, Mathf.Atan2(seg2.y, seg2.x) * Mathf.Rad2Deg);
        }

        private void RotateJointTo(int joint, float degrees)
        {
            // assume degrees = 0 is when at base rotation
            var rot = degrees - (GetJointAngle(joint) - _origJointAngles[joint]);
            for (var i = 0; i <= joint; i++)
            {
                RotateAboutJoint(i, joint, rot);
            }
        }


        private void RotateAboutJoint(int seg, int joint, float degrees)
        {
            _segments[seg].transform.RotateAround(GetJoint(joint), Vector3.forward, degrees);
        }
    }
}
