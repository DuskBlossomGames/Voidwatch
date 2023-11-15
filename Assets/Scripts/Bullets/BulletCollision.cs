using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;

        var damageable = other.GetComponent<Damageable>(); 
        if (damageable != null)
        {
            damageable.Damage(10);
        }
        
        Destroy(gameObject);
        
    }
}
