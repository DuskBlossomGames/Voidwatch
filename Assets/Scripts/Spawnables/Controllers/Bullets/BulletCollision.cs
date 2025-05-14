using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Player;
using Spawnables;
using Spawnables.Player;
using UnityEngine;
using Util;
using static Static_Info.PlayerData;

public class BulletCollision : MonoBehaviour
{
    public float dmg = 10;
    public GameObject owner;
    public float shieldMult, bleedPerc;
    public bool ignoresOwner;
    
    [CanBeNull] public GameObject explosion;
    public float explosionRange;
    public float explosionDmg;

    private bool _leftOwner;
    public bool isKinetic;

    public bool isEnabled = true;
    private List<PlayerDamageType> _damageTypes = new();

    private void Start()
    {
        if (owner.GetComponent<PlayerDamageable>() != null) _damageTypes = PlayerDataInstance.DamageTypes;
    }

    private void OnTriggerExit2D(Collider2D otherCollider)
    {
        if (!isEnabled) return;
        
        if (otherCollider.gameObject == owner)
        {
            _leftOwner = true;
        }

    }
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (!isEnabled) return;
        
        var other = otherCollider.gameObject;
        if (other.layer == LayerMask.NameToLayer("Bullet Detector")) return;

        if (_leftOwner || other != owner)
        {
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 velDiff = other.GetComponent<CustomRigidbody2D>().velocity - GetComponent<CustomRigidbody2D>().velocity;
                float mass = GetComponent<CustomRigidbody2D>().mass;
                float sqrSpeed = velDiff.sqrMagnitude/1_000f;

                var damage = dmg * mass;
                if (isKinetic) damage *= 0.5f * sqrSpeed;
                
                if (damageable.GetType() == typeof(EnemyDamageable)) ((EnemyDamageable) damageable).Damage(damage, gameObject, _damageTypes); 
                else damageable.Damage(damage, gameObject, shieldMult, bleedPerc);
            }

            if (explosion)
            {
                var ignore = new List<Collider2D> { otherCollider };
                if (ignoresOwner) ignore.Add(owner.GetComponent<Collider2D>());
                
                Instantiate(explosion).GetComponent<ExplosionHandler>().Run(explosionDmg, explosionRange, gameObject.layer, ignore);
            }

            Destroy(gameObject);
        }
    }
}
