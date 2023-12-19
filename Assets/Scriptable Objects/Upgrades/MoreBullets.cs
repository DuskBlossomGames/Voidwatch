using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "MoreBullets", menuName = "Upgrades/MoreBullets")]
    public class MoreBullets : BaseUpgrade<ShootEvent>
    {
        public float bulletModifier;

        protected override void OnEvent(Upgradeable upgradeable, ShootEvent evt)
        {
            evt.bulletsPerShot = (int)(evt.bulletsPerShot * bulletModifier);
        }
    }
}