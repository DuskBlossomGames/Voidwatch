using System;
using UnityEngine;

namespace Spawnables.Carcadon
{
    public class CarcadonDamageable : EnemyDamageable
    {
        private CarcadonBrain _cb;

        private void Awake()
        {
            _cb = GetComponent<CarcadonBrain>();
        }

        public override void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            var oh = Health;
            base.Damage(damage, dmgType, reduceMod);
            _cb.TakeDamage(oh, Health);
        }
    }
}