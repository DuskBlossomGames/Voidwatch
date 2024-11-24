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

        public float lockRadius;
        public float attackRadius;
        
        public float rotMoveSpeed, linMoveSpeed; // deg/s, u/s
        public float baseRotMoveSpeed; // rotation of base seg
        
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

        private float GetRotation(int seg, float target, float finalTarget=float.NaN)
        {
            var curRot = NormRot(_segments[seg].transform.rotation.eulerAngles.z - _origRots[seg]);
            var targRot = NormRot(target);
            var finalTargRot = NormRot(float.IsNaN(finalTarget) ? target : finalTarget);
            
            var forbidden = NormRot(foldRotations[seg]);
            var forbidden2 = NormRot(Mathf.Sign(foldRotations[seg]) * -180 + foldRotations[seg]);
                        
            var forbiddenMin = Mathf.Min(forbidden, forbidden2);
            var forbiddenMax = Mathf.Max(forbidden, forbidden2);
                        
            // counterclockwise (positive) rotation
            var ccwTargRot = targRot < curRot ? targRot + 360 : targRot;
            var ccwOpt = ccwTargRot - curRot;

            var a = curRot < forbiddenMin;
            var b = finalTargRot < forbiddenMin;
            var c = curRot < finalTargRot;
            var ccwAllowed = a && (b && c) || !a && (b || c);
            
            var ccwFinalDist = ccwAllowed ? 0 : Mathf.Abs(forbiddenMin - finalTargRot);

            // clockwise (negative) rotation
            var cwTargRot = targRot > curRot ? targRot - 360 : targRot;
            var cwOpt = cwTargRot - curRot;
            
            a = curRot > forbiddenMax;
            b = finalTargRot > forbiddenMax;
            c = curRot > finalTargRot;
            var cwAllowed = a && (b && c) || !a && (b || c);
            
            var cwFinalDist = cwAllowed ? 0 : Mathf.Abs(forbiddenMax - finalTargRot);
            
            return ccwFinalDist < cwFinalDist ? ccwOpt : cwOpt;
        }

        private Vector2? _oldEnd;
        private void Update()
        {
            _attackCooldown.Update();

            var evaluatedDefault = (Vector2)transform.parent.TransformPoint(_defaultPos);

            // fold
            if (_foldDirection != 0 && (GetJoint(0) - evaluatedDefault).magnitude < 0.5f)
            {
                var currentSeg = FloorOrCeil(_foldProgress);
                var rotation = _foldDirection * Mathf.Sign(foldRotations[currentSeg]) * _foldSpeed * Time.deltaTime;

                _foldProgress = MinOrMax(currentSeg, _foldProgress + rotation / foldRotations[currentSeg]);

                RotateJoint(currentSeg, rotation);

                if (_foldProgress <= 0 || _foldProgress >= _segments.Length)
                {
                    _foldProgress = Mathf.Clamp(_foldProgress, 0, _segments.Length);
                    _foldDirection = 0;
                }
            }
            else
            {
                // attack
                var target = /*_target ?? */(Vector2) Player.transform.position;

                if (_attackCooldown.IsFinished && !_target.HasValue && (target - GetJoint(0)).sqrMagnitude < lockRadius * lockRadius)
                {
                    _target = target;
                } else if (_attackCooldown.IsFinished && !_attacking && (target - GetJoint(0)).sqrMagnitude < attackRadius * attackRadius)
                {
                    _attacking = _clawDamage.Active = true;
                }

                if (_attacking)
                {
                    var theta = GetRotation(0, _origRots[0] + foldRotations[0]*3/4);
                    RotateAboutJoint(0, 0, theta * rotMoveSpeed * Time.deltaTime);

                    if (Mathf.Abs(theta) < 2) // close enough, call it done
                    {
                        _attacking = _clawDamage.Active = false;
                        _target = _oldEnd = null;
                        _attackCooldown.Value = Random.Range(attackCooldownMin, attackCooldownMax);
                    }
                }
                else if (!_attackCooldown.IsFinished || (target - GetJoint(_segments.Length - 1)).sqrMagnitude > _totalLengthNoClaw * _totalLengthNoClaw)
                {
                    if (_target.HasValue) _target = null;
                    
                    for (var i = 0; i < _segments.Length; i++)
                    {
                        var theta = GetRotation(i, 0);
                        RotateAboutJoint(i, i, theta * Time.deltaTime);
                        
                        if (i < _segments.Length - 1)
                        {
                            var disp = _origDisps[i] - (Vector2) _segments[i].transform.localPosition;
                            _segments[i].transform.position += (Vector3) disp * (linMoveSpeed * Time.deltaTime);
                        }
                    }
                    
                    for (var i = _segments.Length - 2; i >= 0; i--)
                    {
                        target = GetInverseJoint(i+1);
                        var armStart = GetJoint(i);
                        
                        var dist = target - armStart;
                        _segments[i].transform.position += (Vector3) dist;
                    }
                }
                else
                {
                    // only target if player is in front of the base joint
                    if (transform.parent.InverseTransformPoint(target).y < transform.parent.InverseTransformPoint(GetJoint(_segments.Length - 1)).y)
                    {
                        if (_target.HasValue) _target = null;
                        return;
                    }
                    
                    if ((GetJoint(0) - _oldEnd)?.magnitude < 0.001f)
                    {
                        _attacking = true;
                        return;
                    }
                    _oldEnd = GetJoint(0);
                    
                    { // rotate claw open
                        var theta = GetRotation(0, _origRots[0] + Mathf.Sign(foldRotations[0]) * -180 + foldRotations[0]);
                        RotateAboutJoint(0, 0, theta * rotMoveSpeed * Time.deltaTime);
                    }
                    
                    var origTarget = target;
                    for (var i = 1; i < _segments.Length; i++)
                    {
                        var armStart = GetJoint(i);
                        var armEnd = GetInverseJoint(i);
                        
                        var arm = armEnd - armStart;
                        var displ = target - armStart;

                        var theta = (Mathf.Atan2(displ.y, displ.x) - Mathf.Atan2(arm.y, arm.x)) * Mathf.Rad2Deg;

                        var pDisp = origTarget - armStart;
                        var playerTheta = (Mathf.Atan2(pDisp.y, pDisp.x) - Mathf.Atan2(arm.y, arm.x)) * Mathf.Rad2Deg;
                        
                        theta = GetRotation(i, _segments[i].transform.localRotation.eulerAngles.z + theta - _origRots[i], _segments[i].transform.localRotation.eulerAngles.z + playerTheta - _origRots[i]);
                        
                        var rotSpeed = i == _segments.Length - 1 ? baseRotMoveSpeed : rotMoveSpeed;
                        if (i == 1) RotateJoint(i, (float)Math.Cbrt(theta) * rotSpeed * Time.deltaTime);
                        else RotateAboutJoint(i, i, (float)Math.Cbrt(theta) * rotSpeed * Time.deltaTime);
                        
                        if (i < _segments.Length - 1)
                        {
                            var dist = target - armEnd;
                            var magMod = Mathf.Pow(dist.magnitude, -2/3f);  // mag => cbrt(mag)
                            if (float.IsInfinity(magMod)) magMod = 0;
                            if (i == 1) _segments[0].transform.position += (Vector3) dist * (magMod * linMoveSpeed * Time.deltaTime);
                            _segments[i].transform.position += (Vector3) dist * (magMod * linMoveSpeed * Time.deltaTime);
                        }

                        target = GetJoint(i);
                    }

                    target = GetInverseJoint(_segments.Length - 1);
                    for (var i = _segments.Length - 2; i >= 1; i--)
                    {
                        var armStart = GetJoint(i);

                        var dist = target - armStart;
                        if (i == 1) _segments[0].transform.position += (Vector3) dist;
                        _segments[i].transform.position += (Vector3) dist;

                        target = GetInverseJoint(i);
                    }
                }
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
            if (seg == joint)
            {
                var newAngle = NormRot((_segments[seg].transform.rotation.eulerAngles.z - _origRots[seg]) + degrees);
                
                var forbidden = NormRot(foldRotations[seg]);
                var forbidden2 = NormRot(Mathf.Sign(foldRotations[seg]) * -180 + foldRotations[seg]);

                if (newAngle < Mathf.Max(forbidden, forbidden2) && newAngle > Mathf.Min(forbidden, forbidden2)) return false;
            }
            
            _segments[seg].transform.RotateAround(GetJoint(joint), Vector3.forward, degrees);

            return true;
        }
    }
}