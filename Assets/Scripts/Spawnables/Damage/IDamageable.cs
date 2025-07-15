using UnityEngine;

namespace Spawnables.Damage
{
    public interface IDamageable
    {
        public bool Damage(float damage, GameObject source, float shieldMult, float bleedPerc);
        public bool Damage(float damage, GameObject source);
    }
}
