using System;
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
        public float dodgeDamage;
        public float dodgeExplosionDamage;

        [NonSerialized] public float Health;
        [NonSerialized] public float Scrap;

        [NonSerialized] public short HealthBoosts = 0;
        [NonSerialized] public short DamageBoosts = 0;
        [NonSerialized] public short SpeedBoosts = 0;

    }
}