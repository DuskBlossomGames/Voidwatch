using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Player.Upgrades;
using UnityEngine;
using static Singletons.Static_Info.PlayerData;
using static Singletons.Static_Info.GunInfo;
using Random = UnityEngine.Random;
namespace Player
{
    public static class UpgradePlayer
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public class Rarity
        {
            public static readonly List<Rarity> ALL = new();

            public static readonly Rarity Common = new(0.7f, "common");
            public static readonly Rarity Rare = new(0.27f, "rare");
            public static readonly Rarity Legendary = new(0.03f, "legendary");

            public readonly float Weight;
            public readonly string Name;

            private Rarity(float weight, string name)
            {
                Weight = weight;
                Name = name;
                ALL.Add(this);
            }
        }

        public class Upgrade
        {
            public readonly string Title, Description, Quip;
            public readonly Rarity Rarity;
            public readonly int Cap;
            private readonly Dictionary<UpgradableStat, float> _statMods;
            private readonly Action _applyInternal;

            public Upgrade(string title, string description, string quip, Rarity rarity, int cap, Dictionary<UpgradableStat, float> statMods, Action apply=null)
            {
                Title = title;
                Description = description;
                Quip = quip;
                Rarity = rarity;
                Cap = cap;
                _statMods = statMods;
                _applyInternal = apply;
            }

            public void Apply()
            {
                foreach (var (stat, mod) in _statMods) stat.ApplyUpgrade(mod);
                _applyInternal?.Invoke();
                PlayerDataInstance.Upgrades.Add(this);
            }
        }

        public static readonly Upgrade[] UPGRADES =
        {
            new("Plasma Coil Array",
                "More plasma coils allow for more bullets per shot.",
                "Bigger is better, right?",
                Rarity.Rare,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {GunInfoInstance.bulletsPerShot, 1},
                    {GunInfoInstance.lateralSeparation, 15}
                }),
            new("Streamlined Hull",
                "All projectiles have a chance to be redirected, missing your ship.",
                "Missed me, missed me!",
                Rarity.Rare,
                4,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.missChance, 0.1f}
                }),
            new("Experimental Batteries",
                "These third-party shield batteries hold charge longer, but recharge slower.",
                "Huh, this warning seems to have faded...",
                Rarity.Common,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.maxShield, 1.3f},
                    {PlayerDataInstance.shieldRegenRate, 0.75f}
                }),
            new("Hyperefficient Generators",
                "Improved generators regenerate shields faster.",
                "Why didn't it come with this?",
                Rarity.Common,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.shieldRegenRate, 1.5f}
                }),
            new("Durable Duct Tape",
                "Ship can fire with redirected shield energy when out of ammo, and fire rate is significantly increased.",
                "If we just divert this cable...\nwait, where was that going?",
                Rarity.Legendary,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {GunInfoInstance.fireTime, 0.5f}
                },
                () => { GunInfoInstance.shieldAsAmmo = true; }),
            new("Black Market Mod",
                "Bullets charge in the chamber longer, increasing damage and speed but decreasing fire rate.",
                "I'll just insert this into a critical\nsystem component real quick...",
                Rarity.Rare,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {GunInfoInstance.dmgMod, 1.6f},
                    {GunInfoInstance.shotForce, 1.9f},
                    {GunInfoInstance.fireTime, 3f}
                }),
            new("Voidwrought Accumulator",
                "Attaches to your Void Energy extraction device, increasing Void Energy replenishment speed.",
                "I am one with the Void...",
                Rarity.Common,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.dodgeJuiceRegenRate, 1.25f}
                }),
            new("Laminar Plating",
                "Specialized plating eases the transition to Voidspace, lowering the cost of dashing.",
                "Their world shall be ours.",
                Rarity.Common,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.dodgeJuiceCost, 0.75f}
                }),
            new("Dimensional Folding",
                "Through manipulating folding within Voidspace, you can travel farther in one dash.",
                "One day, I'll become a Voidigami master!",
                Rarity.Rare,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.dodgeDistance, 1.3f}
                }),
            new("High Yield Plasma Coils",
                "Greater plasma harnessing capabilities allow for faster shooting.",
                "More plasma?\nMore bullets. More explosions!",
                Rarity.Common,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {GunInfoInstance.fireTime, 0.8f}
                }),
            new("Interdimensional Spike",
                "This spike touches both our world and theirs, damaging enemies you pass through in Voidspace.",
                "The best offense is an offensive defense.",
                Rarity.Rare,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.dodgeDamage, 50}
                }),
            new("Mining Drill",
                "A frontal drill increases the damage done when ramming ships, and lowers damage taken.",
                "If you squint, a ship\nis basically an asteroid.",
                Rarity.Common,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.collisionDamageMult, 1f},
                    {PlayerDataInstance.asteroidDamageMult, 1.5f},
                    {PlayerDataInstance.takenCollisionDamageMult, 0.65f},
                    {PlayerDataInstance.takenAsteroidCollisionDamageMult, 0.5f}
                }),
            new("Void Energy Sieve",
                "Captures Void Energy as you pass through Voidspace, infusing your next bullets with extra power.",
                "Hippity hoppity, your\nEnergy is my property!",
                Rarity.Rare,
                1,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.postDodgeMult, 2f}
                }),
            new("Intrinsic Refraction",
                "Bullets refract upon hitting their target, launching a copy at the nearest enemy.",
                "You get a bullet! And you get\na bullet! And you get a bullet!",
                Rarity.Legendary,
                0,
                new Dictionary<UpgradableStat, float>
                {
                    {PlayerDataInstance.bulletChains, 1f}
                }),
            new("Void Implant",
                "This Void cybernetic grants brief foresight, instinctively attempting to dodge if you would be hit, at a cost proportional to damage.",
                "No need to worry, I feel\nperfectly f~ AAAHHHHH!",
                Rarity.Legendary,
                1,
                new Dictionary<UpgradableStat, float>(),
                () => { PlayerDataInstance.autoDodge = true; }),
        };

        private static readonly Dictionary<string, List<Upgrade>> BY_RARITY = new();

        static UpgradePlayer()
        {
            foreach (var rarity in Rarity.ALL) BY_RARITY[rarity.Name] = new List<Upgrade>();
            foreach (var upgrade in UPGRADES) BY_RARITY[upgrade.Rarity.Name].Add(upgrade);
        }

        public static Upgrade[] GetRandomUpgrades(int count)
        {
            var upgrades = new List<Upgrade>(count);

            for (var i = 0; i < count; i++)
            {
                var choice = Random.Range(0, Rarity.ALL.Sum(r => r.Weight));
                foreach (var rarity in Rarity.ALL)
                {
                    choice -= rarity.Weight;
                    if (!(choice <= 0)) continue;

                    var selection = BY_RARITY[rarity.Name].FindAll(u => !upgrades.Contains(u) && (u.Cap == 0 || PlayerDataInstance.Upgrades.Count(u2=>u2==u) < u.Cap));
                    upgrades.Add(selection[Random.Range(0, selection.Count)]);
                    break;
                }

                if (upgrades.Count != i+1) throw new Exception("mathematics broke");
            }

            return upgrades.ToArray();
        }
    }
}
