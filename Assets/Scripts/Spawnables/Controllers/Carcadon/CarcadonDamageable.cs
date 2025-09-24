using Spawnables.Damage;
using UnityEngine;
using static Singletons.Static_Info.LevelSelectData;

namespace Spawnables.Controllers.Carcadon
{
    public class CarcadonDamageable : EnemyDamageable
    {
        private CarcadonBrain _cb;

        private void Awake()
        {
            if (LevelSelectDataInstance.hardMode) maxHealth = (int)(maxHealth * LevelSelectDataInstance.hardEliteHealthMod);
            _cb = GetComponent<CarcadonBrain>();
        }

        public override bool Damage(float damage, GameObject source)
        {
            var oh = Health;
            base.Damage(damage, source);
            _cb.TakeDamage(oh, Health);

            return true;
        }
    }
}