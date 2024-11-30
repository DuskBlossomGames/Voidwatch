using System;
using Spawnables;
using Spawnables.Player;
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

        private void Start()
        {
            _leftJaw = transform.GetChild(0);
            _rightJaw = transform.GetChild(1);

            _initialRot = _leftJaw.localRotation.eulerAngles.z;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<Player.Movement>();
            if (!player || player.inputBlocked) return;
            _player = player.transform;

            _clampTimer.Value = grabTime;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent<Player.Movement>(out var movement)) return;
            movement.inputBlocked = false;
            _player.GetComponent<CustomRigidbody2D>().velocity = transform.rotation * new Vector3(ejectionVel, 0, 0);
            _holdTimer.Value = 0;
            _player = null;
        }

        private void Update()
        {
            _clampTimer.Update(_player != null ? -1 : 1);
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
                RotJaw(_leftJaw, 1);
                RotJaw(_rightJaw, -1);
                //_leftJaw.position -= new Vector3(0.03f,0.02f,0);
                //_rightJaw.position -= new Vector3(0.03f,0.02f,0);

                if (_clampTimer.IsFinished)
                {
                    _player.GetComponent<Player.Movement>().inputBlocked = true;
                    _player.GetComponent<CustomRigidbody2D>().velocity = Vector2.zero;
                    _player.GetComponent<PlayerDamageable>().Damage(grabDamage, IDamageable.DmgType.Concussive);
                    wm.BiteCallback();

                    _holdTimer.Value = holdTime;
                }
            }
        }

        private void RotJaw(Transform jaw, int angleSign)
        {
            jaw.RotateAround(
                jaw.position +
                             jaw.localToWorldMatrix.MultiplyVector(new Vector3(-0.5f, 0, 0)),
                Vector3.forward,
                angleSign * _clampTimer.LastStepProgress * (_initialRot - targetRot));

        }
    }
}
