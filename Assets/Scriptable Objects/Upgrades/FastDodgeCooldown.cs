using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "FastDodgeCooldown", menuName = "Upgrades/FastDodgeCooldown")]
    public class FastDodgeCooldown : BaseUpgrade<DodgeEvent>
    {
        public float cooldownModifier;

        protected override void OnEvent(Upgradeable upgradeable, DodgeEvent evt)
        {
            evt.dodgeCooldown *= cooldownModifier;
        }
    }
}