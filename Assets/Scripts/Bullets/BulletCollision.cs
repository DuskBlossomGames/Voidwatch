using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;

        //Psuedocode for damage able objects
        /*if (other.GetComponent<DAMAGEHANDLER>() != null)
        {
            DAMAGECODE
        }*/
        Destroy(gameObject);
        
    }
}
