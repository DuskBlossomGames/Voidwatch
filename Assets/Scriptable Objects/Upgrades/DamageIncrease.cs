using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "DamageIncrease", menuName = "Upgrades/DamageIncrease")]
    public class DamageIncrease : BaseUpgrade
    {
        public float damageDifference;
        
        public override void Equip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().dmgMod += damageDifference;
        }
        
        public override void UnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().dmgMod -= damageDifference;
        }
    }
}