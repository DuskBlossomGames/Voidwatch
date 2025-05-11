using Player;
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
        private float _clampUpdateAmt;
        private readonly Timer _holdTimer = new();
        private float _initialRot;

        public bool HasPlayer => _player != null;

        private void Start()
        {
            _leftJaw = transform.GetChild(0);
            _rightJaw = transform.GetChild(1);

            _clampTimer.Value = _leftJaw.localRotation.eulerAngles.z - targetRot;
            _clampUpdateAmt = _clampTimer.Value / grabTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled) return;
            
            var player = other.GetComponent<Movement>();
            if (!player || player.inputBlocked || player.Dodging) return;
            _player = player.transform;
            
            StartCoroutine(wm.BiteStart());
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled) return;
            
            if (!other.TryGetComponent<Movement>(out var movement)) return;
            movement.inputBlocked = false;
            if (HasPlayer) _player.GetComponent<CustomRigidbody2D>().velocity = transform.rotation * new Vector3(ejectionVel, 0, 0);
            _holdTimer.Value = 0;
            _player = null;
        }

        private void Update()
        {
            _clampTimer.Update(HasPlayer ? -_clampUpdateAmt : _clampUpdateAmt);
            RotTo(targetRot + _clampTimer.Value);
            
            _holdTimer.Update();

            if (_holdTimer.IsActive && _holdTimer.IsFinished)
            {
                _player.GetComponent<Movement>().inputBlocked = false;
                _player.GetComponent<CustomRigidbody2D>().velocity += (Vector2)(transform.rotation * new Vector3(ejectionVel, 0, 0));
                _player = null;

                _holdTimer.Value = 0;
                return;
            }

            if (!_holdTimer.IsActive && _clampTimer.IsFinished)
            {
                _player.GetComponent<Movement>().inputBlocked = true;
                _player.GetComponent<CustomRigidbody2D>().velocity = Vector2.zero;
                _player.GetComponent<PlayerDamageable>().Damage(grabDamage, IDamageable.DmgType.Concussive, gameObject);
                wm.BiteFinish();

                _holdTimer.Value = holdTime;
            }
        }
        
        public void RotTo(float angle)
        {
            _leftJaw.RotateAround(
                _leftJaw.position +
                _leftJaw.localToWorldMatrix.MultiplyVector(new Vector3(-0.5f, 0, 0)),
                Vector3.forward,
                angle - _leftJaw.localRotation.eulerAngles.z);
            
            _rightJaw.RotateAround(
                _rightJaw.position +
                _rightJaw.localToWorldMatrix.MultiplyVector(new Vector3(-0.5f, 0, 0)),
                Vector3.forward,
                -angle - _rightJaw.localRotation.eulerAngles.z);
        }
    }
}
