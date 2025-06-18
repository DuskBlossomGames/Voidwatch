using Player;
using UnityEngine;
using Util;

namespace Spawnables.Damage
{
    public class ElectricArea : MonoBehaviour
    {
        public float shieldDamagePerSecond;
        public float stunTime;

        private Movement _player;
        
        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.TryGetComponent(out _player) || _player.Stunned ||
                !other.TryGetComponent<PlayerDamageable>(out var damageable)) return;

            if (damageable.TakeEMP(shieldDamagePerSecond * Time.deltaTime)) _player.Stun(stunTime);
        }
    }
}