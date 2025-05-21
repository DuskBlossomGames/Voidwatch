using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Static_Info
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData PlayerDataInstance => StaticInfoHolder.Instance.GetCachedComponent<PlayerData>();

        public int maxHealth;
        public float maxShield;
        public float shieldRegenRate;
        public float maxShieldDebt;
        public float maxDodgeJuice;
        public float dodgeJuiceRegenRate;
        public float driftCorrection;
        public float speedLimit;
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
        [NonSerialized] public float Scrap;

        [NonSerialized] public short HealthBoosts = 0;
        [NonSerialized] public short DamageBoosts = 0;
        [NonSerialized] public short SpeedBoosts = 0;

    }
}