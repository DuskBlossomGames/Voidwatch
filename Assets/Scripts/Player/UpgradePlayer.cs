using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Singletons.Static_Info.PlayerData;
using static Singletons.Static_Info.GunInfo;
using Random = UnityEngine.Random;
namespace Player
{
    public class UpgradePlayer
    {
        public class Rarity
        {
            public static readonly List<Rarity> ALL = new();

            public static readonly Rarity Common = new(0.7f, "common");
            public static readonly Rarity Rare = new(0.25f, "rare");
            public static readonly Rarity Legendary = new(0.05f, "legendary");

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
            public readonly bool Unique;
            private Action _applyInternal;

            public Upgrade(string title, string description, string quip, Rarity rarity, bool unique, Action apply)
            {
                Title = title;
                Description = description;
                Quip = quip;
                Rarity = rarity;
                Unique = unique;
                _applyInternal = apply;
            }

            public void Apply()
            {
                _applyInternal();
                PlayerDataInstance.Upgrades.Add(this);
            }
        }

        public static readonly Upgrade[] UPGRADES =
        {
            new("Plasma Coil Array",
                "More plasma coils allows for more bullets per shot.",
                "Bigger is better, right?",
                Rarity.Common,
                false,
                () => {
                    GunInfoInstance.bulletsPerShot = Mathf.Max(2,Mathf.CeilToInt(1.1f * GunInfoInstance.bulletsPerShot));
                    GunInfoInstance.lateralSeperation *= 1.2f;
                }),
            new("Absorptive Plating",
                "Redistributes the energy of incoming bullets, increasing hull integrity and shield capacity.",
                "Can't let one spot hog all the energy. Sharing is caring!",
                Rarity.Common,
                false,
                () => {
                    PlayerDataInstance.Health += Mathf.CeilToInt(0.1f * PlayerDataInstance.maxHealth);
                    PlayerDataInstance.maxHealth *= 1.1f;
                    PlayerDataInstance.maxShield *= 1.05f;
                }),
            new("Experimental Batteries",
                "These third-party shield batteries hold charge longer, but recharge slower.",
                "Huh, this warning seems to have faded...",
                Rarity.Rare,
                false,
                () =>
                {
                    PlayerDataInstance.maxShield *= 1.3f;
                    PlayerDataInstance.maxShieldDebt *= 1.5f;
                    PlayerDataInstance.shieldRegenRate *= .75f;
                }),
            new("Hyperefficient Generators",
                "Improved generators regenerate shields faster.",
                "Why didn't it come with this?",
                Rarity.Common,
                false,
                () => {
                    PlayerDataInstance.shieldRegenRate *= 1.5f;
                }),
            new("Durable Duct Tape",
                "Diverted power massively improves shield capacity and regeneration, but lowers hull integrity.",
                "If we just divert this cable... wait, where was that going?",
                Rarity.Legendary,
                false,
                () => {
                    PlayerDataInstance.shieldRegenRate *= 1.3f;
                    PlayerDataInstance.maxShield *= 1.8f;
                    PlayerDataInstance.maxShieldDebt *= .2f;
                    PlayerDataInstance.maxHealth *= 0.7f;
                    PlayerDataInstance.Health = Mathf.Min(PlayerDataInstance.Health, PlayerDataInstance.maxHealth);
                }),
            new("Black Market Mod",
                "Bullets charge in the chamber longer, increasing damage and speed but decreasing fire rate.",
                "I'll just insert this into a critical system component real quick...",
                Rarity.Legendary,
                false,
                () => {
                    GunInfoInstance.dmgMod *= 1.6f;
                    GunInfoInstance.shotForce *= 1.9f;
                    GunInfoInstance.fireTime += 0.4f;
                }),
            new("Voidwrought Accumulator",
                "Attaches to your Void Energy eXtraction (V.E.X.) device, increasing Void Energy replenishment speed.",
                "I am one with the Void...",
                Rarity.Common,
                false,
                () => {
                    PlayerDataInstance.dodgeJuiceRegenRate *= 1.25f;
                }),
            new("Laminar Plating",
                "Specialized plating eases the transition to Voidspace, lowering the cost of jaunts.",
                "Their world shall be ours.",
                Rarity.Common,
                false,
                () => {
                    PlayerDataInstance.dodgeJuiceCost *= 0.75f;
                }),
            new("Containment Matrix",
                "Allows Voidhawk starships to hold more Void Energy without leaking.",
                "Hey, stop running away! Get back here!",
                Rarity.Common,
                false,
                () => {
                    PlayerDataInstance.maxDodgeJuice *= 1.35f;
                }),
            new("High Yield Plasma Coils",
                "Greater plasma harnessing capabilities allows for faster shooting.",
                "More plasma? More bullets. More explosions!",
                Rarity.Rare,
                false,
                () => {
                    GunInfoInstance.fireTime *= 0.8f;
                }),
            new("Interdimensional Spike",
                "This spike touches both our world and theirs, damaging enemies you pass through in Voidspace.",
                "The best offense is an offensive defense.",
                Rarity.Rare,
                false,
                () =>
                {
                    PlayerDataInstance.dodgeDamage += 50;
                }),
            new("Mining Drill",
                "A frontal drill increases the damage done when ramming ships.",
                "If you squint, a ship is basically an asteroid.",
                Rarity.Common,
                false,
                () =>
                {
                    PlayerDataInstance.collisionDamageMult += 1f;
                }),
            new("Void Energy Sieve",
                "Captures Void Energy as you pass through Voidspace, infusing your next bullets with extra power.",
                "Hippity hoppity, your Energy is my property!",
                Rarity.Rare,
                true,
                () =>
                {
                    PlayerDataInstance.postDodgeMult = 1.5f;
                }),
            new("Intrinsic Refraction",
                "Bullets refract upon hitting their target, launching a copy at the nearest enemy.",
                "You get a bullet! And you get a bullet! And you get a bullet!",
                Rarity.Rare,
                false,
                () =>
                {
                    PlayerDataInstance.bulletChains += 1;
                }),
            new("Void Implant",
                "This Void cybernetic grants brief foresight, instinctively attempting to dodge if you would be hit, at a cost proportional to damage.",
                "No need to worry, I feel perfectly f~ AAAHHHHH!",
                Rarity.Legendary,
                true,
                () =>
                {
                    PlayerDataInstance.autoDodge = true;
                }),
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

                    var selection = BY_RARITY[rarity.Name].FindAll(u => !upgrades.Contains(u) && (!u.Unique || !PlayerDataInstance.Upgrades.Contains(u)));
                    upgrades.Add(selection[Random.Range(0, selection.Count)]);
                    break;
                }

                if (upgrades.Count != i+1) throw new Exception("mathematics broke");
            }

            return upgrades.ToArray();
        }
    }
}
