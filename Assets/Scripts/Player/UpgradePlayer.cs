using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Static_Info.PlayerData;
using static Static_Info.GunInfo;
using Random = UnityEngine.Random;
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
        public readonly Action Apply;

        public Upgrade(string title, string description, string quip, Rarity rarity, Action apply)
        {
            Title = title;
            Description = description;
            Quip = quip;
            Rarity = rarity;
            Apply = apply;
        }
    }

    public static readonly Upgrade[] UPGRADES =
    {
        new("Plasma Coil Array",
            "More plasma coils allows for more bullets per shot.",
            "Bigger is better, right?",
            Rarity.Common,
            () => {
                GunInfoInstance.bulletsPerShot = Mathf.Max(2,Mathf.CeilToInt(1.1f * GunInfoInstance.bulletsPerShot));
                GunInfoInstance.lateralSeperation *= 1.1f;
            }),
        new("Absorptive Plating",
            "Redistributes the energy from bullets, increasing hull integrity and shield capacity.",
            "Can't let one spot hog all the energy. Sharing is caring!",
            Rarity.Rare,
            () => {
                PlayerDataInstance.Health += Mathf.CeilToInt(0.1f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxHealth = Mathf.CeilToInt(1.1f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxShield = Mathf.CeilToInt(1.05f * PlayerDataInstance.maxShield);
            }),
        new("Experimental Batteries",
            "These third-party shield batteries hold longer, but recharge slower.",
            "Huh, this warning seems to have faded...",
            Rarity.Legendary,
            () => {
                PlayerDataInstance.maxShield = Mathf.CeilToInt(1.2f * PlayerDataInstance.maxShield);
                PlayerDataInstance.maxShieldDebt *= 1.5f;
                PlayerDataInstance.shieldRegenRate *= .8f;
            }),
        new("Hyperefficient Generators",
            "Improved generators regenerate shields faster.",
            "Why didn't it come with this?",
            Rarity.Common,
            () => {
                PlayerDataInstance.shieldRegenRate *= 1.4f;
            }),
        new("Durable Duct Tape",
            "Diverted power massively improves shield capacity and regeneration, but lowers hull integrity.",
            "If we just divert this cable... wait, where was that going?",
            Rarity.Common,
            () => {
                PlayerDataInstance.shieldRegenRate *= 1.3f;
                PlayerDataInstance.maxShield = Mathf.CeilToInt(1.5f * PlayerDataInstance.maxShield);
                PlayerDataInstance.maxShieldDebt *= .2f;
                PlayerDataInstance.maxHealth = Mathf.CeilToInt(.7f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.Health = Mathf.Min(PlayerDataInstance.Health, PlayerDataInstance.maxHealth);
            }),
        new("Black Market Mod",
            "Bullets charge in the chamber longer, increasing damage and speed but decreasing fire rate.",
            "I'll just insert this into a critical system component real quick...",
            Rarity.Common,
            () => {
                GunInfoInstance.dmgMod *= 1.2f;
                GunInfoInstance.shotForce *= 1.2f;
                GunInfoInstance.fireTime += 0.5f;
            }),
        new("Voidwrought Accumulator",
            "Attaches to your Void Energy eXtraction (V.E.X.) device, increasing Void Energy replenishment speed.",
            "The Void surrounds us...",
            Rarity.Common,
            () => {
                PlayerDataInstance.dodgeJuiceRegenRate *= 1.15f;
            }),
        new("Laminar Plating",
            "Specialized plating eases the transition to Voidspace, lower the cost of jaunts.",
            "Their world shall be ours.",
            Rarity.Common,
            () => {
                PlayerDataInstance.dodgeJuiceCost *= 0.85f;
            }),
        new("Containment Matrix",
            "Allows Voidhawk starships to hold more Void Energy without leaking.",
            "Hey, stop running away! Get back here!",
            Rarity.Common,
            () => {
                PlayerDataInstance.maxDodgeJuice *= 1.3f;
            }),
        new("High Yield Plasma Coils",
            "Greater plasma harnessing capabilities allows for faster shooting.",
            "More plasma? More bullets. More explosions!",
            Rarity.Common,
            () => {
                GunInfoInstance.fireTime *= 0.8f;
            }),
        new("Cyclic Firing Chamber",
            "The chamber cycles several times per shot, sending out more waves of bullets.",
            "No, no, no! The bullets are supposed to go forward.",
            Rarity.Common,
            () => {
                GunInfoInstance.repeats = Mathf.Max(Mathf.CeilToInt(1.5f * GunInfoInstance.repeats),1);
            }),
        new("T.U.R.B.O.",
            "Turbo capabilities increase the Infiltrator's movement speed.",
            "Technically Unregulated Rapid Boost Orifice",
            Rarity.Common,
            () => {
                PlayerDataInstance.speedLimit *= 1.2f;
            }),
        new("Coolant Reserves",
            "Extra coolant allows the engine to push further, achieving greater acceleration.",
            "Overheating, you say? Faster!",
            Rarity.Common,
            () => {
                PlayerDataInstance.acceleration = Mathf.CeilToInt(1.2f* PlayerDataInstance.acceleration);
                PlayerDataInstance.driftCorrection *= 1.2f;
            }),
        new("Interdimensional Spike",
            "This spike touches both our world and theirs, damaging enemies you pass through in Voidspace.",
            "The best offense is an offensive defense.",
            Rarity.Common,
            () =>
            {
                PlayerDataInstance.dodgeDamage += 200;
            }),
        new("Automatic Incendiary Device",
            "Attaching to the void jaunt module, this device will trigger a timed detonation upon entering Voidspace.",
            "The AID is superior to any human aide for aiding in detonations.",
            Rarity.Common,
            () =>
            {
                PlayerDataInstance.dodgeExplosionDamage += 300;
            }),
    };

    public static readonly Dictionary<string, List<Upgrade>> BY_RARITY = new();

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

                var selection = BY_RARITY[rarity.Name].FindAll(u => !upgrades.Contains(u));
                upgrades.Add(selection[Random.Range(0, selection.Count)]);
                break;
            }

            if (upgrades.Count != i+1) throw new Exception("mathematics broke");
        }

        return upgrades.ToArray();
    }
}
