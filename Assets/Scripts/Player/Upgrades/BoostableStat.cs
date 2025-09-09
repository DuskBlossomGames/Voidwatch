using System;
using System.Collections.Generic;
using UnityEngine;
using static Singletons.Static_Info.PlayerData;

namespace Player.Upgrades
{
    public interface IBoostableStat
    {
        protected static readonly float[] BOOST_AMOUNTS = { 1.2f, 1.5f, 2f };
        public static readonly int[] BOOST_COSTS = { 100, 300, 900 };

        protected static readonly List<IBoostableStat> STATS = new();
        public static List<IBoostableStat> Stats
        {
            get
            {
                for (var i = STATS.Count - 1; i >= 0; i--)
                {
                    for (var j = i-1; j >= 0; j--)
                    {
                        if (STATS[i].GetName() != STATS[j].GetName()) continue;
                        
                        STATS.RemoveAt(j);
                        i--;
                    }
                }
                return new List<IBoostableStat>(STATS);
            }
        }
        
        public string GetName();
        public  void Boost();
        public int GetBoosts();
    }
    
    [Serializable]
    public class BoostableStat<T> : UpgradableStat<T>, IBoostableStat
    {

        public string name;
        public string GetName() => name;
        
        public int Boosts { get; private set; }
        public int GetBoosts() => Boosts;

        public virtual void Boost()
        {
            Upgrades[0] = IBoostableStat.BOOST_AMOUNTS[Boosts] - (interaction == Interaction.Additive ? 1 : 0);
            Boosts++;
        }

        public BoostableStat()
        {
            IBoostableStat.STATS.Add(this);
            Upgrades.Add(interaction == Interaction.Multiplicative ? 1 : 0);
        }
    }

    [Serializable]
    public class MaxHealthStat : BoostableStat<int>
    {
        public override void Boost()
        {
            var dmg = (int) PlayerDataInstance.maxHealth - PlayerDataInstance.Health;
            base.Boost();
            PlayerDataInstance.Health = (int) PlayerDataInstance.maxHealth - dmg;
        }

        public override void ApplyUpgrade(float mod)
        {
            var dmg = (int) PlayerDataInstance.maxHealth - PlayerDataInstance.Health;
            base.ApplyUpgrade(mod);
            PlayerDataInstance.Health = (int) PlayerDataInstance.maxHealth - dmg;
        }
    }
}