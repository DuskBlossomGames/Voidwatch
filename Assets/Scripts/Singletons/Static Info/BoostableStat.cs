using System;
using System.Collections.Generic;
using static Singletons.Static_Info.PlayerData;

namespace Singletons.Static_Info
{
    [Serializable]
    public abstract class BoostableStat
    {
        protected static readonly float[] BOOST_AMOUNTS = { 1f, 1.2f, 1.5f, 2f };
        
        public static readonly int[] BOOST_COSTS = { 100, 300, 900 };
        public static readonly List<BoostableStat> STATS = new ();
        
        public virtual int Boosts { get; set; }
        public string name;
    }
    
    [Serializable]
    public class BoostableStat<T> : BoostableStat
    {
        public float value;

        public BoostableStat() => STATS.Add(this);

        private static T AsType(float value)
        {
            return typeof(T) == typeof(int) ? (T) (object) (int) value : (T) (object) value;
        }
        private static float AsFloat(T value)
        {
            return typeof(T) == typeof(int) ? (int) (object) value : (float) (object) value;
        }

        public static implicit operator T(BoostableStat<T> stat) => AsType(stat.value * BOOST_AMOUNTS[stat.Boosts]);
        
        public static BoostableStat<T> operator *(BoostableStat<T> stat, float b) => new()
        {
            name = stat.name,
            value = AsFloat(AsType(stat.value * b)),
            Boosts = stat.Boosts
        };
        public static BoostableStat<T> operator /(BoostableStat<T> stat, float b) => new()
        {
            name = stat.name,
            value = AsFloat(AsType(stat.value / b)),
            Boosts = stat.Boosts
        };
        public static BoostableStat<T> operator +(BoostableStat<T> stat, float b) => new()
        {
            name = stat.name,
            value = AsFloat(AsType(stat.value + b)),
            Boosts = stat.Boosts
        };
        public static BoostableStat<T> operator -(BoostableStat<T> stat, float b) => new()
        {
            name = stat.name,
            value = AsFloat(AsType(stat.value - b)),
            Boosts = stat.Boosts
        };
    }

    [Serializable]
    public class MaxHealthStat : BoostableStat<int>
    {
        private int _boosts;
        public override int Boosts
        {
            get => _boosts;
            set
            {
                var dmg = (int) PlayerDataInstance.maxHealth - PlayerDataInstance.Health;
                _boosts = value;
                PlayerDataInstance.Health = (int) PlayerDataInstance.maxHealth - dmg;
            }
        }

        public static MaxHealthStat operator *(MaxHealthStat stat, float b) => new()
        {
            name = stat.name,
            value = (int) (stat.value * b),
            Boosts = stat.Boosts
        };
        public static MaxHealthStat operator /(MaxHealthStat stat, float b) => new()
        {
            name = stat.name,
            value = (int) (stat.value / b),
            Boosts = stat.Boosts
        };
        public static MaxHealthStat operator +(MaxHealthStat stat, float b) => new()
        {
            name = stat.name,
            value = (int) (stat.value + b),
            Boosts = stat.Boosts
        };
        public static MaxHealthStat operator -(MaxHealthStat stat, float b) => new()
        {
            name = stat.name,
            value = (int) (stat.value - b),
            Boosts = stat.Boosts
        };
    }
}