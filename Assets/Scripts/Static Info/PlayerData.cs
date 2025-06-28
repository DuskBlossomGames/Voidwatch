using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using Shop;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Static_Info
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData PlayerDataInstance => StaticInfoHolder.Instance.GetCachedComponent<PlayerData>();

        public AssetLabelReference borderSprites, upgradeSprites, boostableStatSprite;
        
        public MaxHealthStat maxHealth;
        public BoostableStat<float> maxShield;
        public float shieldRegenRate;
        public float maxShieldDebt;
        public BoostableStat<float> maxDodgeJuice;
        public float dodgeJuiceRegenRate;
        public float driftCorrection;
        public BoostableStat<float> speedLimit;
        public float acceleration;
        public float dodgeRedirectPercentage;
        public float dodgeJuiceCost;
        public float dodgeVelocity;
        public float dodgeDistance;
        public float dodgeCooldown;
        
        [Space(10)]
        [Header("Upgrade Values (uninitialized)")]
        public float dodgeDamage;
        public float dodgeExplosionDamage;
        public float postDodgeMult = 1;
        public float collisionDamageMult = 1;
        public int bulletChains;
        public bool healthPickupsEnabled;
        public bool autoDodge;
        public readonly List<PlayerDamageType> DamageTypes = new();

        public readonly List<UpgradePlayer.Upgrade> Upgrades = new();

        [NonSerialized] public float Health;
        [NonSerialized] public int Scrap;
        
        public readonly Dictionary<string, Sprite> UpgradeSprites = new();
        public readonly Dictionary<string, Sprite[]> RaritySprites = new();
        public readonly Dictionary<string, Sprite> BoostableStatSprites = new();
        private void Awake()
        {
            Addressables.LoadAssetsAsync<Sprite>(borderSprites, null).Completed += handle =>
            {
                var byName = handle.Result.ToDictionary(s => s.name, s => s);
                foreach (var rarity in UpgradePlayer.Rarity.ALL)
                {
                    RaritySprites[rarity.Name] = new[] { byName[rarity.Name], byName[rarity.Name + "-BOX"] };
                }
            };
            Addressables.LoadAssetsAsync<Sprite>(upgradeSprites, null).Completed += handle =>
            {
                var byName = handle.Result.ToDictionary(s => s.name, s => s);
                foreach (var upgrade in UpgradePlayer.UPGRADES)
                {
                    UpgradeSprites[upgrade.Title] = byName[upgrade.Title];
                }
            };
            Addressables.LoadAssetsAsync<Sprite>(boostableStatSprite, null).Completed += handle =>
            {
                var byName = handle.Result.ToDictionary(s => s.name, s => s);
                foreach (var upgrade in UpgradePlayer.UPGRADES)
                {
                    BoostableStatSprites[upgrade.Title] = byName[upgrade.Title];
                }
            };
        }
    }
}