using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "FastDashCooldown", menuName = "Upgrades/FastDashCooldown")]
    public class FastDashCooldown : BaseUpgrade
    {
        public float cooldownDifference;
        
        public override void Equip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<Movement>().dodgeCooldown += cooldownDifference;
        }

        public override void UnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<Movement>().dodgeCooldown -= cooldownDifference;
        }
    }
}