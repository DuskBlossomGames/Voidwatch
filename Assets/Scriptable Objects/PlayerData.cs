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

        [NonSerialized] public float? Health = null;
        [NonSerialized] public float Scrap;
    }
}