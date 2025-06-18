using UnityEngine;

namespace Spawnables.Damage
{
    public interface IDamageable
    {
        public void Damage(float damage, GameObject source, float shieldMult, float bleedPerc);
        public void Damage(float damage, GameObject source);
    }
}
