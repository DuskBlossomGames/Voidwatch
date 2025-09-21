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

        private Vector2 _myVel;
        private void Update()
        {
            _cooldownTimer.Update();

            _myVel = _rb.linearVelocity; // "cache" it from before the collision
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.attachedRigidbody == null) return;
            var vel = (other.attachedRigidbody.linearVelocity - _myVel).magnitude;

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

            var isAsteroid = damageable.GetComponent<AsteroidController>() != null;
            var mult = isAsteroid
                ? PlayerDataInstance.asteroidDamageMult
                : PlayerDataInstance.collisionDamageMult;
            
            if (isAsteroid) vel = _myVel.magnitude; // make it not relative so that asteroids are consistent
            damageable.Damage(mult * enemyMod * vel, gameObject);

            if (!damageable.IsDead && !isAsteroid) // asteroid handles it itself
            {
                _dmgable.Damage(PlayerDataInstance.takenCollisionDamageMult * playerMod * vel, other.gameObject, shieldMult, bleedPerc);
                _cooldownTimer.Value = cooldown;
            }
            
        }
    }
}