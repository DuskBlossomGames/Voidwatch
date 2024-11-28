using System;
using Spawnables;
using Spawnables.Player;
using UnityEngine;
using Util;
using static Static_Info.PlayerData;

namespace Player
{
    public class PlayerCollision : MonoBehaviour
    {
        public float enemyMod, playerMod;
        public float trueDamageMod;
        public float cooldown;
        public float minExplosionVel;
        public GameObject explosionObj;

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
            damageable.Damage(enemyMod * vel, IDamageable.DmgType.Concussive);

            if (_rb.velocity.magnitude >= minExplosionVel && damageable.IsDead)
            {
                var explosion = Instantiate(explosionObj);
                explosion.SetActive(true);
                explosion.transform.position = damageable.transform.position;
                explosion.transform.localScale = new Vector3(damageable.transform.lossyScale.x,
                    1, damageable.transform.lossyScale.y);
                explosion.GetComponent<ExplosionHandler>().Run();
                explosion.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                _dmgable.Damage(playerMod * vel, trueDamageMod * vel, IDamageable.DmgType.Concussive);                    
            }
                
            _cooldownTimer.Value = cooldown;
        }
    }
}