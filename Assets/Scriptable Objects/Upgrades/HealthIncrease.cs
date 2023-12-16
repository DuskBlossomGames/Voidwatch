using Player;
using Spawnables;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    [CreateAssetMenu(fileName = "HealthIncrease", menuName = "Upgrades/HealthIncrease")]
    public class HealthIncrease : BaseUpgrade
    {
        public PlayerData playerData;
        public int healthDifference;
        
        public override void OnEquip(Upgradeable upgradeable)
        { 
            playerData.playerMaxHealth += healthDifference;
        }
        
        public override void OnUnequip(Upgradeable upgradeable)
        {
            playerData.playerMaxHealth -= healthDifference;
        }
    }
}