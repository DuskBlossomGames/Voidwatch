using System;
using Spawnables.Player;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Asteroids
{
    public class AsteroidController : EnemyDamageable
    {
        public Vector2 startVel;
        private CustomRigidbody2D _rb;

        private LineRenderer _posVec, _velVec;
        public Material material;
        
        private new void Start()
        {
            base.Start();
            _rb = GetComponent<CustomRigidbody2D>();

            _rb.velocity = startVel;
        }

        private void Update()
        {
            var angle = Mathf.Sign(Vector3.SignedAngle(_rb.position, _rb.velocity, Vector3.forward)) * 90;
            _rb.velocity = Vector3.RotateTowards(_rb.velocity, Quaternion.Euler(0, 0, angle) * _rb.position, 0.15f*Mathf.PI*Time.deltaTime, 0);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent<PlayerDamageable>(out var player)) return;

            player.Damage(20, IDamageable.DmgType.Physical);
        }

        protected override void OnDeath()
        {
            base.OnDeath(); // spawn bits
        }
    }
}