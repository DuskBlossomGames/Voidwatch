using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "MoreBullets", menuName = "Upgrades/MoreBullets")]
    public class MoreBullets : BaseUpgrade
    {
        public int bulletDifference;
        
        public override void Equip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().bulletsPerShot += bulletDifference;
        }
        
        public override void UnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().bulletsPerShot -= bulletDifference;
        }
    }
}