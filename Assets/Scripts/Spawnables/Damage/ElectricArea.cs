using System;
using Player;
using UnityEngine;

namespace Spawnables.Damage
{
    public class ElectricArea : MonoBehaviour
    {
        public float initialDamage;
        public float shieldDamagePerSecond;
        public float stunTime;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Movement>(out var player) || player.Stunned ||
                !other.TryGetComponent<PlayerDamageable>(out var damageable)) return;

            if (damageable.TakeEMP(initialDamage)) player.Stun(stunTime);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.TryGetComponent<Movement>(out var player) || player.Stunned ||
                !other.TryGetComponent<PlayerDamageable>(out var damageable)) return;

            if (damageable.TakeEMP(shieldDamagePerSecond * Time.deltaTime)) player.Stun(stunTime);
        }
    }
}