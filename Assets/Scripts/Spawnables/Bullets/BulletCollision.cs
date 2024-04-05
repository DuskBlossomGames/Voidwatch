using System;
using System.Linq;
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
                //Debug.Log(string.Format(".05 * dmg * mass * sqrSpeed = .05 * {0} * {1} * {2} = {3}",dmg,mass,sqrSpeed,.05f * dmg * mass * sqrSpeed));
                if (isKinetic) { damageable.Damage(.5f * evt.damage * mass * sqrSpeed, dmgType); }
                else {           damageable.Damage(evt.damage * mass, dmgType); }
                
            }

            Destroy(gameObject);
        }
        
    }
}
