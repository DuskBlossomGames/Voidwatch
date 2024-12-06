using System;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Static_Info
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData PlayerDataInstance => StaticInfoHolder.instance.GetCachedComponent<PlayerData>();

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
        public System.Collections.Generic.List<BaseComponent> weapons;

        [NonSerialized] public System.Collections.Generic.List<UpgradeInstance> Upgrades;
        [NonSerialized] public float Health;
        [NonSerialized] public float Scrap;

        [NonSerialized] public short healthBoosts = 0;
        [NonSerialized] public short damageBoosts = 0;
        [NonSerialized] public short speedBoosts = 0;

    }
}
