using System;
using System.Collections;
using Spawnables;
using Spawnables.Player;
using TMPro;
using UnityEngine;
using Util;

namespace Bosses.Worm
{
    public class JawGrab : MonoBehaviour
    {
        public float grabDamage, grabTime, holdTime, targetRot, ejectionVel;
        public WormBrain wm;

        private Transform _leftJaw, _rightJaw, _player;
        private readonly Timer _clampTimer = new();
        private readonly Timer _holdTimer = new();
        private float _initialRot;

        public bool HasPlayer => _player != null;

        private void Start()
        {
            _leftJaw = transform.GetChild(0);
            _rightJaw = transform.GetChild(1);

            _initialRot = _leftJaw.localRotation.eulerAngles.z;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled) return;
            
            var player = other.GetComponent<Player.Movement>();
            if (!player || player.inputBlocked || player.Dodging) return;
            _player = player.transform;
            
            _clampTimer.Value = grabTime;
            wm.BiteStart();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled) return;
            
            if (_player == null || !other.TryGetComponent<Player.Movement>(out var movement)) return;
            movement.inputBlocked = false;
            _player.GetComponent<CustomRigidbody2D>().velocity = transform.rotation * new Vector3(ejectionVel, 0, 0);
            _holdTimer.Value = 0;
            _player = null;
        }

        private void Update()
        {
            _clampTimer.Update(HasPlayer ? -1 : 1);
            _holdTimer.Update();

            if (_holdTimer.IsActive && _holdTimer.IsFinished)
            {
                _player.GetComponent<Player.Movement>().inputBlocked = false;
                _player.GetComponent<CustomRigidbody2D>().velocity += (Vector2)(transform.rotation * new Vector3(ejectionVel, 0, 0));
                _player = null;

                _holdTimer.Value = 0;
                return;
            }

            if (!_holdTimer.IsActive && _clampTimer.IsActive)
            {
                RotBy(_clampTimer.LastStepProgress * (_initialRot - targetRot));
                //_leftJaw.position -= new Vector3(0.03f,0.02f,0);
                //_rightJaw.position -= new Vector3(0.03f,0.02f,0);

                if (_clampTimer.IsFinished)
                {
                    _player.GetComponent<Player.Movement>().inputBlocked = true;
                    _player.GetComponent<CustomRigidbody2D>().velocity = Vector2.zero;
                    _player.GetComponent<PlayerDamageable>().Damage(grabDamage, IDamageable.DmgType.Concussive, gameObject);
                    wm.BiteFinish();

                    _holdTimer.Value = holdTime;
                }
            }
        }
        
        public void RotBy(float angle)
        {
            _leftJaw.RotateAround(
                _leftJaw.position +
                _leftJaw.localToWorldMatrix.MultiplyVector(new Vector3(-0.5f, 0, 0)),
                Vector3.forward,
                angle);
            
            _rightJaw.RotateAround(
                _rightJaw.position +
                _rightJaw.localToWorldMatrix.MultiplyVector(new Vector3(-0.5f, 0, 0)),
                Vector3.forward,
                -angle);
        }
    }
}
