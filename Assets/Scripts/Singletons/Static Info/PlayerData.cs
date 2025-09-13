using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using Player.Upgrades;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Singletons.Static_Info
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData PlayerDataInstance => StaticInfoHolder.Instance.GetCachedComponent<PlayerData>();

        public AssetLabelReference borderSprites, upgradeSprites, boostableStatSprite;

        [NonSerialized] public bool IsTutorial;
        
        public MaxHealthStat maxHealth;
        public BoostableStat<float> maxShield;
        public UpgradableStat<float> shieldRegenRate;
        public BoostableStat<float> maxDodgeJuice;
        public UpgradableStat<float> dodgeJuiceRegenRate;
        public float driftCorrection;
        public BoostableStat<float> speedLimit;
        public float acceleration;
        public float dodgeRedirectPercentage;
        public UpgradableStat<float> dodgeJuiceCost;
        public float dodgeVelocity;
        public UpgradableStat<float> dodgeDistance;
        public float dodgeCooldown;

        [Space(10)] [Header("Upgrade Values (uninitialized)")]
        public UpgradableStat<float> missChance;
        public UpgradableStat<float> dodgeDamage;
        public float dodgeExplosionDamage;
        public UpgradableStat<float> postDodgeMult;
        public UpgradableStat<float> collisionDamageMult;
        public UpgradableStat<float> asteroidDamageMult;
        public UpgradableStat<float> takenCollisionDamageMult;
        public UpgradableStat<float> takenAsteroidCollisionDamageMult;
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
                foreach (var stat in IBoostableStat.Stats)
                {
                    BoostableStatSprites[stat.GetName()] = byName[stat.GetName()];
                }
            };
        }
    }
}