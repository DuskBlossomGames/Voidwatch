using System;
using System.Linq;
using JetBrains.Annotations;
using Player;
using Scriptable_Objects;
using Scriptable_Objects.Upgrades;
using Spawnables;
using UnityEngine;
using Util;

public class BulletCollision : MonoBehaviour
{
    public float dmg = 10;
    public GameObject owner;
    public IDamageable.DmgType dmgType;
    public int weaponID;
    public bool ignoresOwner;
    
    [CanBeNull] public GameObject explosion;
    public float explosionRange;
    public float explosionDmg;

    private bool _leftOwner;
    public bool isKinetic;
    private Upgradeable _upgradeable;

    private void Start()
    {
        _upgradeable = owner.GetComponent<Upgradeable>();
    }

    private void OnTriggerExit2D(Collider2D otherCollider)
    {
        if (otherCollider.gameObject == owner)
        {
            _leftOwner = true;
        }

    }
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        var other = otherCollider.gameObject;
        if (other.layer == LayerMask.NameToLayer("Bullet Detector")) return;

        if (_leftOwner || other != owner)
        {
            var evt = new DealDamageEvent { damaged = other, damage = dmg };
            if (_upgradeable) _upgradeable.HandleEvent(evt, weaponID);
            
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 velDiff = other.GetComponent<CustomRigidbody2D>().velocity - GetComponent<CustomRigidbody2D>().velocity;
                float mass = GetComponent<CustomRigidbody2D>().mass;
                float sqrSpeed = velDiff.sqrMagnitude/1_000f;

                if (isKinetic) { damageable.Damage(.5f * evt.damage * mass * sqrSpeed, dmgType, gameObject); }
                else {           damageable.Damage(evt.damage * mass, dmgType, gameObject); }
                
            }

            if (explosion)
            {
                explosion.transform.parent = null;
                explosion.GetComponent<ExplosionHandler>().Run();
                explosion.GetComponent<ParticleSystem>().Play();
                
                const int rayNum = 16;
                for (var i = 0; i < rayNum; i++)
                {
                    var raydir = new Vector2(Mathf.Cos(2 * Mathf.PI * i / rayNum),
                        Mathf.Sin(2 * Mathf.PI * i / rayNum));
                    LayerMask mask = 1 << LayerMask.NameToLayer("Enemies") | 1 << LayerMask.NameToLayer("Player") |
                                     1 << LayerMask.NameToLayer("Scene Objects");
                    var hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + explosionRange * raydir,
                        mask);
                    if (hit.collider == null || hit.collider == otherCollider) continue;
                    if (ignoresOwner && hit.collider == owner.GetComponent<Collider2D>()) continue;
                    
                    hit.transform.GetComponent<IDamageable>()?.Damage(explosionDmg, dmgType, gameObject);
                }
            }

            Destroy(gameObject);
        }
    }
}
