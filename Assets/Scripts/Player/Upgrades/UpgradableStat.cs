using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using static Singletons.Static_Info.PlayerData;

namespace Player.Upgrades
{
    public enum Interaction { Multiplicative, Additive }

    [Serializable]
    public class UpgradableStat
    {
        public /*protected readonly*/ List<float> Upgrades = new();

        public virtual void ApplyUpgrade(float mod)
        {
            Upgrades.Add(mod);
        }
    }
    
    [Serializable]
    public class UpgradableStat<T> : UpgradableStat
    {
        public float baseValue, minValue;
        public Interaction interaction;
        
        private float CalculateValue()
        {
            return Mathf.Max(minValue, Upgrades.Aggregate(baseValue,
                (current, modVal) => interaction == Interaction.Multiplicative ? current * modVal : current + modVal));
        }
        
        private static T AsType(float value)
        {
            return typeof(T) == typeof(int) ? (T) (object) (int) value : (T) (object) value;
        }
        public static implicit operator T(UpgradableStat<T> stat) => AsType(stat.CalculateValue());
    }
}