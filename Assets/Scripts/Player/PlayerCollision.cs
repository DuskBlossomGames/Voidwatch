using System;
using Spawnables;
using Spawnables.Asteroids;
using Spawnables.Player;
using UnityEngine;
using Util;

namespace Player
{
    public class PlayerCollision : MonoBehaviour
    {
        public float enemyMod, playerMod;
        public float trueDamageMod;
        public float cooldown;

        private readonly Timer _cooldownTimer = new();
        private Movement _movement;
        private PlayerDamageable _dmgable;
        private CustomRigidbody2D _rb;
        
        private void Start()
        {
            _movement = transform.parent.GetComponent<Movement>();
            _dmgable = transform.parent.GetComponent<PlayerDamageable>();
            _rb = transform.parent.GetComponent<CustomRigidbody2D>();
        }

        private void Update()
        {
            _cooldownTimer.Update();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_cooldownTimer.IsFinished || _movement.Dodging) return;
            if (!other.gameObject.TryGetComponent<Damageable>(out var damageable)) return;
            if (damageable.GetComponent<MissleAim>() != null) return;
                
            var vel = (other.attachedRigidbody.velocity - _rb.velocity).magnitude;
            damageable.Damage(enemyMod * vel, IDamageable.DmgType.Concussive, gameObject);

            if (!damageable.IsDead && other.gameObject.GetComponent<AsteroidController>() == null) // asteroid handles it itself
            {
                _dmgable.Damage(playerMod * vel, IDamageable.DmgType.Concussive, gameObject);
                _cooldownTimer.Value = cooldown;
            }
            
        }
    }
}