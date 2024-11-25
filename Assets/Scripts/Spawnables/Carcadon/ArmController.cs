using System;
using System.Linq;
using Spawnables.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

namespace Spawnables.Carcadon
{
    public class ArmController : MonoBehaviour
    {
        // all arrays start at the outside (claw) and work in
        public Vector2[] joints;
        public float[] foldRotations;
        public float timeToFold;

        public float attackCooldownMin, attackCooldownMax;

        public float timeToOpen, timeToClose, timeToReturn;
        private float _attackProgress;
        
        private GameObject[] _segments;
        private Vector2[] _inverseJoints; // doesn't exist for claw
        
        private float _foldProgress;
        private float _foldSpeed;
        private int _foldDirection;

        private bool _attacking;
        private Vector2? _target;

        private GameObject _player;
        public GameObject Player
        {
            get => _player;
            set
            {
                _player = value;
                GetComponentInChildren<ClawDamage>().Player = value.GetComponent<PlayerDamageable>();
            }
        }

        private Vector2 _defaultPos;

        private float _totalLengthNoClaw;
        
        private float[] _origRots;
        private Vector2[] _origDisps;

        private ClawDamage _clawDamage;

        private readonly Timer _attackCooldown = new();
        
        private void Start()
        {
            _clawDamage = GetComponentInChildren<ClawDamage>();
            
            _segments = new [] {transform.GetChild(3).gameObject, transform.GetChild(2).gameObject,
                transform.GetChild(1).gameObject, transform.GetChild(0).gameObject};
            
            for (var i = 1; i < _segments.Length; i++) _totalLengthNoClaw += _segments[i].GetComponentInChildren<BoxCollider2D>().size.y * _segments[i].transform.lossyScale.y;
            
            _inverseJoints = new Vector2[joints.Length];
            for (var i = 1; i < joints.Length; i++)
            {
                _inverseJoints[i] = _segments[i].transform.InverseTransformPoint(GetJoint(i - 1));
            }
            
            _origRots = new float[joints.Length];
            _origDisps = new Vector2[joints.Length];
            for (var i = 0; i < joints.Length; i++)
            {
                _origRots[i] = _segments[i].transform.rotation.eulerAngles.z;
                _origDisps[i] = _segments[i].transform.localPosition;
            }

            _defaultPos = transform.parent.InverseTransformPoint(GetJoint(0));
            
            _foldSpeed = foldRotations.Sum(Mathf.Abs) / timeToFold;
        }

        public void FoldClosed() { _foldDirection = 1; }
        public void FoldOpen() { _foldDirection = -1; }
        
        private float MinOrMax(float a, float b)
        {
            return _foldDirection == 1 ? (a + 1 < b ? a + 1 : b) : (a > b ? a : b);
        }

        private int FloorOrCeil(float a)
        {
            return _foldDirection == 1 ? (int)a : Mathf.CeilToInt(a - 1);
        }

        private bool _folded;
        private void Update()
        {
            _attackCooldown.Update();
            
            // fold
            if (_attackProgress == 0 && _foldDirection != 0)
            {
                var currentSeg = FloorOrCeil(_foldProgress);
                var rotation = _foldDirection * Mathf.Sign(foldRotations[currentSeg]) * _foldSpeed * Time.deltaTime;

                _foldProgress = MinOrMax(currentSeg, _foldProgress + rotation / foldRotations[currentSeg]);

                RotateJoint(currentSeg, rotation);

                if (_foldProgress <= 0 || _foldProgress >= _segments.Length)
                {
                    _foldProgress = Mathf.Clamp(_foldProgress, 0, _segments.Length);
                    _folded = _foldDirection == 1;
                    _foldDirection = 0;
                }
            }

            var xBound = transform.parent.InverseTransformPoint(GetJoint(0)).x;
            var yBound = transform.parent.InverseTransformPoint(GetJoint(_segments.Length - 1)).y;
            var playerPos = transform.parent.InverseTransformPoint(_player.transform.position);
            
            var inXBound = (int) Mathf.Sign(playerPos.x) == (int) Mathf.Sign(xBound) && Mathf.Abs(playerPos.x) < Mathf.Abs(xBound);
            var inYBound = playerPos.y > yBound && playerPos.y - yBound <= 0.35f;
            
            if (!_folded && _attackCooldown.IsFinished && ((inXBound && inYBound) || _attackProgress != 0))
            {
                if (_attackProgress == 0) _clawDamage.Active = true;
                
                var openBase = (Mathf.Sign(foldRotations[^1]) * -180 + foldRotations[^1]) * 3/4;
                var openNext = Mathf.Sign(foldRotations[^2]) * -180 + foldRotations[^2];

                var returnNext = -openNext;
                var foldLast = foldRotations[^3]/2;
                var foldClaw = foldRotations[0]/2;

                var returnBase = -openBase;
                var returnLast = -foldLast;
                var returnClaw = -foldClaw;
                
                if (_attackProgress < timeToOpen)
                {
                    RotateJoint(_segments.Length - 1, openBase / timeToOpen * Time.deltaTime);
                    RotateJoint(_segments.Length - 2, openNext / timeToOpen * Time.deltaTime);
                } else if (_attackProgress - timeToOpen < timeToClose)
                {
                    RotateJoint(_segments.Length - 2, returnNext / timeToClose * Time.deltaTime);
                    RotateJoint(_segments.Length - 3, foldLast / timeToClose * Time.deltaTime);
                    RotateJoint(0, foldClaw / timeToClose * Time.deltaTime);
                } else if (_attackProgress - timeToOpen - timeToClose < timeToReturn)
                {
                    RotateJoint(_segments.Length - 1, returnBase / timeToReturn * Time.deltaTime);
                    RotateJoint(_segments.Length - 3, returnLast / timeToReturn * Time.deltaTime);
                    RotateJoint(0, returnClaw / timeToReturn * Time.deltaTime);
                }
                else
                {
                    _clawDamage.Active = false;
                    _attackProgress = 0;
                    _attackCooldown.Value = Random.Range(attackCooldownMin, attackCooldownMax);
                }

                _attackProgress += Time.deltaTime;
            }
        }
        
        // end joint
        private Vector2 GetInverseJoint(int i)
        {
            Debug.Assert(i != 0);
            return _segments[i].transform.TransformPoint(_inverseJoints[i]);
        }
        // start joint
        private Vector2 GetJoint(int i)
        {
            return _segments[i].transform.TransformPoint(joints[i]);
        }
        
        private void RotateJoint(int joint, float degrees)
        {
            for (var i = 0; i <= joint; i++)
            {
                // failed, undo the whole rotation and exit
                if (!RotateAboutJoint(i, joint, degrees))
                {
                    for (var j = i; j >= 0; j--)
                    {
                        RotateAboutJoint(j, joint, -degrees);
                    }
                    return;
                }
            }
        }

        private float NormRot(float degrees)
        {
            return (degrees%360 + 360)%360;
        }

        private bool RotateAboutJoint(int seg, int joint, float degrees)
        {
            // if (seg == joint)
            // {
            //     var newAngle = NormRot((_segments[seg].transform.rotation.eulerAngles.z - _origRots[seg]) + degrees);
            //     
            //     var forbidden = NormRot(foldRotations[seg]);
            //     var forbidden2 = NormRot(Mathf.Sign(foldRotations[seg]) * -180 + foldRotations[seg]);
            //
            //     if (newAngle < Mathf.Max(forbidden, forbidden2) && newAngle > Mathf.Min(forbidden, forbidden2)) return false;
            // }
            
            _segments[seg].transform.RotateAround(GetJoint(joint), Vector3.forward, degrees);

            return true;
        }
    }
}