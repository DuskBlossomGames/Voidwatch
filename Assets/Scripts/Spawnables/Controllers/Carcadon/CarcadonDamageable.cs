using Spawnables.Damage;
using UnityEngine;

namespace Spawnables.Controllers.Carcadon
{
    public class CarcadonDamageable : EnemyDamageable
    {
        private CarcadonBrain _cb;

        private void Awake()
        {
            _cb = GetComponent<CarcadonBrain>();
        }

        public override void Damage(float damage, GameObject source)
        {
            var oh = Health;
            base.Damage(damage, source);
            _cb.TakeDamage(oh, Health);
        }
    }
}