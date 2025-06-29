using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Player;
using Spawnables.Controllers.Asteroids;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using UnityEngine;
using Util;
using static Singletons.Static_Info.PlayerData;

namespace Spawnables.Controllers.Bullets
{
    public class BulletCollision : MonoBehaviour
    {
        public bool scaleWithDamage;
        public float dmg = 10;
        public GameObject owner;
        public int chains;
        public float shieldMult, bleedPerc;
        public bool ignoresOwner;

        private GameObject _firstCollider;
    
        [CanBeNull] public GameObject explosion;
        public float explosionRange;
        public float explosionDmg;

        private bool _leftFirstCollider;
        public bool isKinetic;

        public bool isEnabled = true;
        private List<PlayerDamageType> _damageTypes = new();

        private void Start()
        {
            if (_firstCollider == null) _firstCollider = owner;
            if (owner != null && owner.GetComponent<PlayerDamageable>() != null) _damageTypes = PlayerDataInstance.DamageTypes;

            if (scaleWithDamage)
            {
                transform.localScale *= dmg / 15;
                GetComponent<TrailRenderer>().widthMultiplier *= dmg / 15;
            }
        }

        private void OnTriggerExit2D(Collider2D otherCollider)
        {
            if (!isEnabled) return;
        
            if (otherCollider.gameObject == _firstCollider)
            {
                _leftFirstCollider = true;
            }

        }
        private void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (!isEnabled) return;
        
            var other = otherCollider.gameObject;
            if (other.layer == LayerMask.NameToLayer("Bullet Detector")) return;
            if (!_leftFirstCollider && other == _firstCollider) return;
            
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 velDiff = other.GetComponent<CustomRigidbody2D>().linearVelocity - GetComponent<CustomRigidbody2D>().linearVelocity;
                float mass = GetComponent<CustomRigidbody2D>().mass;
                float sqrSpeed = velDiff.sqrMagnitude/1_000f;

                var damage = dmg * mass;
                if (isKinetic) damage *= 0.5f * sqrSpeed;
                
                if (damageable.GetType() == typeof(EnemyDamageable)) ((EnemyDamageable) damageable).Damage(damage, gameObject, _damageTypes); 
                else damageable.Damage(damage, gameObject, shieldMult, bleedPerc);
                
                if (chains == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    try
                    {
                        var nearest = FindObjectsByType<EnemyDamageable>(FindObjectsSortMode.None)
                            .Where(d => d.gameObject != other && d.GetType() != typeof(AsteroidController))
                            .OrderByDescending(e => ((Vector2)(e.transform.position - transform.position)).sqrMagnitude)
                            .Last();
                        
                        chains -= 1;
                        _firstCollider = other;

                        GetComponent<CustomRigidbody2D>().linearVelocity =
                            GetComponent<CustomRigidbody2D>().linearVelocity.magnitude * (nearest.transform.position - transform.position).normalized;
                    }
                    catch (InvalidOperationException) {}
                }
            }
            else
            {
                Destroy(gameObject);
            }

            if (explosion)
            {
                var ignore = new List<Collider2D> { otherCollider };
                if (ignoresOwner) ignore.Add(owner.GetComponent<Collider2D>());
                
                var obj = Instantiate(explosion);
                obj.transform.position = transform.position;
                obj.GetComponent<ExplosionHandler>().Run(explosionDmg, explosionRange, gameObject, ignore);
            }
        }
    }
}
