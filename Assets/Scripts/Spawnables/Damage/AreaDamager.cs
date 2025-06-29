using UnityEngine;

namespace Spawnables.Damage
{
    public class AreaDamager : MonoBehaviour
    {
        public float damagePerSecond;
        public float entryDamage;
        public float shieldMult, bleedPerc;
        public bool canOnlyHurtPlayer = false;
        public GameObject owner;

        void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<IDamageable>(out var dmg) && !canOnlyHurtPlayer || canOnlyHurtPlayer && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                dmg.Damage(damagePerSecond * Time.deltaTime, gameObject, shieldMult, bleedPerc);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<IDamageable>(out var dmg) && !canOnlyHurtPlayer || canOnlyHurtPlayer && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                dmg.Damage(entryDamage, gameObject, shieldMult, bleedPerc);
            }
        }
    }
}