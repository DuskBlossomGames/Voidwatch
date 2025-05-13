using Player;
using UnityEngine;

namespace Spawnables
{
    public interface IDamageable
    {
        public void Damage(float damage, GameObject source, float shieldMult, float bleedPerc);
        public void Damage(float damage, GameObject source);
    }
}
