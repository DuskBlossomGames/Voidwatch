using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "BulletSpread", menuName = "Upgrades/BulletSpread")]
    public class BulletSpread : BaseUpgrade<ShootEvent>
    {
        public float separationModifier;

        protected override void OnEvent(Upgradeable upgradeable, ShootEvent evt)
        {
            evt.lateralSeperation *= separationModifier;
        }
    }
}