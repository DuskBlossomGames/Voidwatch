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
        public bool hasAttack;
        
        public float attackCooldown;

        public float timeToOpen, timeToClose, timeToReturn;
        private float _attackProgress;
        
        private GameObject[] _segments;
        
        private float _foldProgress;
        private float _foldSpeed;
        private int _foldDirection;

        private bool _attacking;
        private Vector2? _target;

        private GameObject _player;
        public GameObject Player
        {
            set
            {
                _player = value;
                var cd = GetComponentInChildren<ClawDamage>();
                if (cd != null) cd.Player = value.GetComponent<PlayerDamageable>();
            }
        }
        
        private ClawDamage _clawDamage;

        private readonly Timer _attackCooldown = new();
        
        private void Start()
        {
            _clawDamage = GetComponentInChildren<ClawDamage>();
            
            _segments = new [] {transform.GetChild(3).gameObject, transform.GetChild(2).gameObject,
                transform.GetChild(1).gameObject, transform.GetChild(0).gameObject};
            
            
            _foldSpeed = foldRotations.Sum(Mathf.Abs) / timeToFold;
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
                for (var i = 0; i < _segments.Length; i++) RotateJoint(i, foldRotations[i]);
            }
            else
            {
                for (var i = _segments.Length - 1; i >= 0; i--) RotateJoint(i, -foldRotations[i]);
            }
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

            if (!hasAttack) return;

            var xBound = transform.parent.InverseTransformPoint(GetJoint(0)).x;
            var yBound = transform.parent.InverseTransformPoint(GetJoint(_segments.Length - 2)).y;
            var playerPos = transform.parent.InverseTransformPoint(_player.transform.position);
            
            var inXBound = (int) Mathf.Sign(playerPos.x) == (int) Mathf.Sign(xBound) && Mathf.Abs(playerPos.x) < Mathf.Abs(xBound);
            var inYBound = playerPos.y > yBound && playerPos.y - yBound <= 1;
            
            if (!_folded && _attackCooldown.IsFinished && ((inXBound && inYBound) || _attackProgress != 0))
            {
                if (_attackProgress == 0)
                {
                    _clawDamage.Active = true;
                    _clawDamage.transform.parent.GetChild(1).gameObject.SetActive(true); // enable trail renderer
                }
                
                var openBase = (Mathf.Sign(foldRotations[^1]) * -180 + foldRotations[^1]) * 3/4;
                var openNext = Mathf.Sign(foldRotations[^2]) * -180 + foldRotations[^2];

                var returnNext = -openNext;
                var foldLast = foldRotations[^3]/2;
                var foldClaw = foldRotations[0] * 3/4;

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
                    _clawDamage.transform.parent.GetChild(1).gameObject.SetActive(false); // disable trail renderer
                    _attackProgress = 0;
                    _attackCooldown.Value = attackCooldown;
                }
                
                if (_attackCooldown.IsFinished) _attackProgress += Time.deltaTime; // don't increment if we just set to 0
            }
        }
        
        private Vector2 GetJoint(int i)
        {
            return _segments[i].transform.TransformPoint(joints[i]);
        }
        
        private void RotateJoint(int joint, float degrees)
        {
            for (var i = 0; i <= joint; i++) RotateAboutJoint(i, joint, degrees);
        }


        private void RotateAboutJoint(int seg, int joint, float degrees)
        {
            _segments[seg].transform.RotateAround(GetJoint(joint), Vector3.forward, degrees);
        }
    }
}