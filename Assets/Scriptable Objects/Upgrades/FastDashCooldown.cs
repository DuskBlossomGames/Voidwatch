using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "FastDashCooldown", menuName = "Upgrades/FastDashCooldown")]
    public class FastDashCooldown : BaseUpgrade
    {
        public float cooldownDifference;
        
        public override void OnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<Movement>().dodgeCooldown += cooldownDifference;
        }

        public override void OnUnequip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<Movement>().dodgeCooldown -= cooldownDifference;
        }
    }
}