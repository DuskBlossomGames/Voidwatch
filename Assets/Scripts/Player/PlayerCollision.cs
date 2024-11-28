using System;
using Spawnables;
using UnityEngine;

namespace Player
{
    public class PlayerCollision : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.TryGetComponent<Damageable>(out var damageable))
            {
                print("hit "+other.gameObject.name);
            }
        }
    }
}