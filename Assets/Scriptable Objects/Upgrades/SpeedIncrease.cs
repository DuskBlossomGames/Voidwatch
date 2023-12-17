using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "SpeedIncrease", menuName = "Upgrades/SpeedIncrease")]
    public class SpeedIncrease : BaseUpgrade<MoveEvent>
    {
        public float accelerationModifier;
        public float speedLimitModifier;

        protected override void OnEvent(Upgradeable upgradeable, MoveEvent evt)
        {
            evt.acceleration *= accelerationModifier;
            evt.speedLimit *= speedLimitModifier;
        }
    }
}