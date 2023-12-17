using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "DamageIncrease", menuName = "Upgrades/DamageIncrease")]
    public class DamageIncrease : BaseUpgrade<DealDamageEvent>
    {
        public float damageModifier;

        protected override void OnEvent(Upgradeable upgradeable, DealDamageEvent evt)
        {
            evt.damage *= damageModifier;
        }
    }
}