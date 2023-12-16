using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "BulletSpread", menuName = "Upgrades/BulletSpread")]
    public class BulletSpread : BaseUpgrade
    {
        public float separationDifference;
        
        public override void OnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().lateralSeperation += separationDifference;
        }
        
        public override void OnUnequip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<GunHandler>().lateralSeperation -= separationDifference;
        }
    }
}