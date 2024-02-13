using System;
using UnityEngine;

namespace Scriptable_Objects
{
    public class PlayerData : ScriptableObject
    {
        public int playerMaxHealth;
        public float playerMaxShield;
        public float playerShieldRegenRate;
        public float playerMaxShieldDebt;

        public System.Collections.Generic.List<UpgradeInstance> upgrades;
        public System.Collections.Generic.List<BaseComponent> weapons;

        [NonSerialized] public float? Health = null;
        [NonSerialized] public float Scrap;
    }
}