using Spawnables.Damage;
using UnityEngine;

namespace Spawnables.Controllers.Chasers
{
    public class ChaserDamageable : EnemyDamageable
    {
        public override void Damage(float damage, GameObject source)
        {
            var cb = GetComponent<ChaserBehavior>();
            cb.TakeDamage();
            base.Damage(damage, source);
        }
    }
}
