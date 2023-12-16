using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "MoreBullets", menuName = "Upgrades/MoreBullets")]
    public class MoreBullets : BaseUpgrade
    {
        public int bulletDifference;
        
        public override void OnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().bulletsPerShot += bulletDifference;
        }
        
        public override void OnUnequip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().bulletsPerShot -= bulletDifference;
        }
    }
}