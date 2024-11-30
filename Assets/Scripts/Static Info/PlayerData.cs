using System;
using UnityEngine;
using Util;

namespace Static_Info
{
    public class PlayerData : MonoBehaviour
    {
        public static PlayerData PlayerDataInstance => StaticInfoHolder.instance.GetCachedComponent<PlayerData>();
        
        public int playerMaxHealth;
        public float playerMaxShield;
        public float playerShieldRegenRate;
        public float playerMaxShieldDebt;
        public System.Collections.Generic.List<BaseComponent> weapons;
        
        [NonSerialized] public System.Collections.Generic.List<UpgradeInstance> Upgrades;
        [NonSerialized] public float Health;
        [NonSerialized] public float Scrap;

        [NonSerialized] public short healthBoosts = 0;
        [NonSerialized] public short damageBoosts = 0;
        [NonSerialized] public short speedBoosts = 0;
        
    }
}