using System;
using Spawnables.Player;
using UnityEngine;
using Util;

namespace EnemySpawner
{
    public class HealthPickupController : MonoBehaviour
    {
        public float healthGain;
        public float swellScale, swellTime, shrinkTime;

        private readonly Timer _swell = new();
        private int _dir = 1;
        private float _startScale;

        private float _startShrink;
        private float _shrinkScale = 0;
        private void Start()
        {
            _swell.Value = swellTime/2;
            _swell.SetValue(0);
            
            _startScale = transform.localScale.x;
        }

        private void Update()
        {
            if (_shrinkScale != 0)
            {
                transform.localScale = new Vector3(_shrinkScale, _shrinkScale, 1);
                _shrinkScale -= _startShrink / shrinkTime * Time.deltaTime;

                if (_shrinkScale <= 0) Destroy(gameObject);
                return;
            }
            
            _swell.Update(_dir);
            if (_swell.Value == _swell.MaxValue || _swell.Value == 0) _dir *= -1;

            var scale = Mathf.SmoothStep(_startScale, _startScale * swellScale, _swell.Progress);
            transform.localScale = new Vector3(scale, scale, 1);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerDamageable>(out var player)) return;

            player.Heal(healthGain);
            GetComponent<Collider2D>().enabled = false;
            
            _shrinkScale = _startShrink = transform.localScale.x;
        }
    }
}