using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "DamageIncrease", menuName = "Upgrades/DamageIncrease")]
    public class DamageIncrease : BaseUpgrade
    {
        public float damageDifference;
        
        public override void OnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().dmgMod += damageDifference;
        }
        
        public override void OnUnequip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().dmgMod -= damageDifference;
        }
    }
}