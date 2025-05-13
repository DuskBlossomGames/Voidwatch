using System;
using Spawnables.Player;
using UnityEngine;

namespace Spawnables.Carcadon
{
    public class ClawDamage : MonoBehaviour
    {
        public float damage;
        public float shieldMult, bleedPerc;
        
        [NonSerialized] public PlayerDamageable Player;
        [NonSerialized] public bool Active;
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (Active && other.gameObject == Player.gameObject)
            {
                Player.Damage(damage, gameObject, shieldMult, bleedPerc);
                Active = false;
            }
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (Active && other.gameObject == Player.gameObject)
            {
                Player.Damage(damage, gameObject, shieldMult, bleedPerc);
                Active = false;
            }
        }
    }
}