using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    public float dmg = 10;
    bool _playerHurtable;
    private void Start()
    {
        if(gameObject.layer==LayerMask.NameToLayer("Player Bullets"))
        {
            _playerHurtable = false;
            //Debug.Log("Disabled collision for bullet");
        } else {
            _playerHurtable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D otherCollider)
    {
        if (!_playerHurtable && gameObject.layer == LayerMask.NameToLayer("Player Bullets") 
            && otherCollider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            //Debug.Log(string.Format("Enabled collision for bullet from {0}", LayerMask.LayerToName(otherCollider.gameObject.layer)));
            _playerHurtable = true;
        }

    }
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        GameObject other = otherCollider.gameObject;

        if (_playerHurtable && other.layer == LayerMask.NameToLayer("Player") || other.layer != LayerMask.NameToLayer("Player"))
        {
            /*Debug.Log(string.Format("Collided with object on layer: {0}; is player = {1}", 
                LayerMask.LayerToName(other.layer),
                other.layer == LayerMask.NameToLayer("Player")));*/
            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                Vector2 velDiff = other.GetComponent<Rigidbody2D>().velocity - GetComponent<Rigidbody2D>().velocity;
                float mass = GetComponent<Rigidbody2D>().mass;
                float sqrSpeed = velDiff.sqrMagnitude/1_000f;
                //Debug.Log(string.Format(".05 * dmg * mass * sqrSpeed = .05 * {0} * {1} * {2} = {3}",dmg,mass,sqrSpeed,.05f * dmg * mass * sqrSpeed));
                damageable.Damage(.5f * dmg * mass * sqrSpeed) ;
            }
            var wdamageable = other.GetComponent<WormDamageable>();
            if (wdamageable != null)
            {
                Vector2 velDiff = other.GetComponent<Rigidbody2D>().velocity - GetComponent<Rigidbody2D>().velocity;
                float mass = GetComponent<Rigidbody2D>().mass;
                float sqrSpeed = velDiff.sqrMagnitude / 1_000f;
                //Debug.Log(string.Format(".05 * dmg * mass * sqrSpeed = .05 * {0} * {1} * {2} = {3}",dmg,mass,sqrSpeed,.05f * dmg * mass * sqrSpeed));
                wdamageable.Damage(.5f * dmg * mass * sqrSpeed);
            }

            Destroy(gameObject);
        }
        
    }
}
