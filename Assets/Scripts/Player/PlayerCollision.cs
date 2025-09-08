using Spawnables.Controllers.Asteroids;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using UnityEngine;
using Util;
using static Singletons.Static_Info.PlayerData;

namespace Player
{
    public class PlayerCollision : MonoBehaviour
    {
        public float enemyMod, playerMod, planetMod;
        public float trueDamageMod;
        public float cooldown;
        public float shieldMult, bleedPerc;

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
            var vel = (other.attachedRigidbody.linearVelocity - _rb.linearVelocity).magnitude;

            if (!other.gameObject.TryGetComponent<Damageable>(out var damageable))
            {
                if (other.gameObject.layer == LayerMask.NameToLayer("Scene Objects"))
                {
                    _dmgable.Damage(PlayerDataInstance.takenCollisionDamageMult * playerMod * planetMod * vel, other.gameObject, shieldMult, bleedPerc);
                }
                return;
            }

            if (_movement.Dodging)
            {
                if (PlayerDataInstance.dodgeDamage > 0)
                {
                    damageable.Damage(PlayerDataInstance.dodgeDamage, gameObject);
                    GetComponentInParent<PlayerVFXController>().RunSpike();
                }
                return;
            }
            
            if (!_cooldownTimer.IsFinished) return;
            if (damageable.GetComponent<MissleAim>() != null) return;
                
            var mult = damageable.GetComponent<AsteroidController>() != null
                ? PlayerDataInstance.asteroidDamageMult
                : PlayerDataInstance.collisionDamageMult;
            damageable.Damage(mult * enemyMod * vel, gameObject);

            if (!damageable.IsDead && other.gameObject.GetComponent<AsteroidController>() == null) // asteroid handles it itself
            {
                _dmgable.Damage(PlayerDataInstance.takenCollisionDamageMult * playerMod * vel, other.gameObject, shieldMult, bleedPerc);
                _cooldownTimer.Value = cooldown;
            }
            
        }
    }
}