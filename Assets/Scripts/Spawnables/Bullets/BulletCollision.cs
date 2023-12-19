using System;
using System.Linq;
using Player;
using Scriptable_Objects;
using Spawnables;
using UnityEngine;
using Util;

public class BulletCollision : MonoBehaviour
{
    public float dmg = 10;
    public GameObject owner;

    private bool _leftOwner;
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

        if (_leftOwner || other != owner)
        {
            var damage = dmg;

            if (_upgradeable)
            {
                damage = _upgradeable.Upgrades.Aggregate(damage,
                    (current, upgrade) => upgrade.OnDealDamage(other, current));
            }
            
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector2 velDiff = other.GetComponent<CustomRigidbody2D>().velocity - GetComponent<CustomRigidbody2D>().velocity;
                float mass = GetComponent<CustomRigidbody2D>().mass;
                float sqrSpeed = velDiff.sqrMagnitude/1_000f;
                damageable.Damage(.5f * damage * mass * sqrSpeed, IDamageable.DmgType.Physical) ;
            }

            Destroy(gameObject);
        }
        
    }
}
