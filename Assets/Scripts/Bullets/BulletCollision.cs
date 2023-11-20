using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    public int dmg = 10;
    bool _playerHurtable;
    private void Start()
    {
        if(gameObject.layer==LayerMask.NameToLayer("Player Bullets"))
        {
            _playerHurtable = false;
            Debug.Log("Disabled collision for bullet");
        } else {
            _playerHurtable = true;
        }
    }
    private void OnTriggerExit2D(Collider2D otherCollider)
    {
        if (!_playerHurtable && gameObject.layer == LayerMask.NameToLayer("Player Bullets") 
            && otherCollider.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log(string.Format("Enabled collision for bullet from {0}", LayerMask.LayerToName(otherCollider.gameObject.layer)));
            _playerHurtable = true;
        }

    }
    private void OnTriggerEnter2D(Collider2D otherCollider)
    {
        GameObject other = otherCollider.gameObject;

        if (_playerHurtable && other.layer == LayerMask.NameToLayer("Player") || other.layer != LayerMask.NameToLayer("Player"))
        {
            Debug.Log(string.Format("Collided with object on layer: {0}; is player = {1}", 
                LayerMask.LayerToName(other.layer),
                other.layer == LayerMask.NameToLayer("Player")));
            var damageable = other.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.Damage(dmg);
            }

            Destroy(gameObject);
        }
        
    }
}
