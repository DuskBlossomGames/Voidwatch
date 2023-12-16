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
        
        public override void Equip(Upgradeable upgradeable)
        { 
            playerData.playerMaxHealth += healthDifference;
        }
        
        public override void UnEquip(Upgradeable upgradeable)
        {
            playerData.playerMaxHealth -= healthDifference;
        }
    }
}