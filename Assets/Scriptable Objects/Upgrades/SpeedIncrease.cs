using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "SpeedIncrease", menuName = "Upgrades/SpeedIncrease")]
    public class SpeedIncrease : BaseUpgrade
    {
        public float accelerationDifference;
        public float speedLimitDifference;
        
        public override void OnEquip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<Movement>().acceleration += accelerationDifference;
            upgradeable.GetComponent<Movement>().speedLimit += speedLimitDifference;
        }
        
        public override void OnUnequip(Upgradeable upgradeable)
        {
            upgradeable.GetComponent<Movement>().acceleration -= accelerationDifference;
            upgradeable.GetComponent<Movement>().speedLimit -= speedLimitDifference;
        }
    }
}