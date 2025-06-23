using System;
using System.Timers;
using Player;
using UnityEngine;
using Timer = Util.Timer;

namespace Spawnables.Damage
{
    public class ElectricArea : MonoBehaviour
    {
        public float initialDamage;
        public float shieldDamagePerSecond;
        public float stunTime;
        public float immunityTime;

        private readonly Timer _immunity = new();
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<Movement>(out var player) || player.Stunned || !_immunity.IsFinished ||
                !other.TryGetComponent<PlayerDamageable>(out var damageable)) return;

            if (damageable.TakeEMP(initialDamage))
            {
                _immunity.Value = immunityTime + stunTime;
                player.Stun(stunTime);
            }
            else player.GetComponent<PlayerVFXController>().RunBif();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.TryGetComponent<Movement>(out var player) || player.Stunned || !_immunity.IsFinished ||
                !other.TryGetComponent<PlayerDamageable>(out var damageable)) return;

            if (damageable.TakeEMP(shieldDamagePerSecond * Time.deltaTime))
            {
                _immunity.Value = immunityTime + stunTime;
                player.Stun(stunTime);
            }
        }
    }
}
