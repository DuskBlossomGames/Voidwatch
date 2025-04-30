using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Static_Info.PlayerData;
using static Static_Info.GunInfo;

public class UpgradePlayer
{
    public enum Upgrades
    {
        Wave,
        Solidify,
        Tank,
        Surge,
        Overcharge,
        Lance,
        Juicesqueezer,
        Juicedynamic,
        Juicebox,
        Reload,
        Repeater,
        Speedboost,
        Handling,

    }

    public static void Upgrade(Upgrades upgrade)
    {
        switch (upgrade)
        {
            case Upgrades.Wave:
                GunInfoInstance.bulletsPerShot = Mathf.Max(2,Mathf.CeilToInt(1.1f * GunInfoInstance.bulletsPerShot));
                GunInfoInstance.lateralSeperation *= 1.1f;
                break;
            case Upgrades.Solidify: //general health boost
                PlayerDataInstance.Health += Mathf.CeilToInt(0.1f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxHealth = Mathf.CeilToInt(1.1f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxShield = Mathf.CeilToInt(1.05f * PlayerDataInstance.maxShield);
                break;
            case Upgrades.Tank: //Shield boost but reduce regen
                PlayerDataInstance.maxShield = Mathf.CeilToInt(1.2f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxShieldDebt *= 1.5f;
                PlayerDataInstance.shieldRegenRate *= .8f;
                break;
            case Upgrades.Surge: //Increase shield regen speed
                PlayerDataInstance.shieldRegenRate *= 1.4f;
                break;
            case Upgrades.Overcharge: //Decrease health, decrease debt, massively increase regen and shields
                PlayerDataInstance.shieldRegenRate *= 1.3f;
                PlayerDataInstance.maxShield = Mathf.CeilToInt(1.5f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxShieldDebt *= .2f;
                PlayerDataInstance.maxHealth = Mathf.CeilToInt(.7f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.Health = Mathf.Min(PlayerDataInstance.Health, PlayerDataInstance.maxHealth);
                break;
            case Upgrades.Lance:
                GunInfoInstance.dmgMod *= 1.2f;
                GunInfoInstance.shotForce *=1.2f;
                GunInfoInstance.fireTime += 0.5f;
                break;
            case Upgrades.Juicesqueezer:
                PlayerDataInstance.dodgeJuiceRegenRate *= 1.15f;
                break;
            case Upgrades.Juicedynamic:
                PlayerDataInstance.dodgeJuiceCost *= 0.85f;
                break;
            case Upgrades.Juicebox:
                PlayerDataInstance.maxDodgeJuice *= 1.3f;
                break;
            case Upgrades.Reload:
                GunInfoInstance.fireTime *= 0.8f;
                break;
            case Upgrades.Repeater:
                GunInfoInstance.repeats = Mathf.Max(Mathf.CeilToInt(1.5f * GunInfoInstance.repeats),1);
                break;
            case Upgrades.Speedboost:
                PlayerDataInstance.speedLimit *= 1.2f;
                break;
            case Upgrades.Handling:
                PlayerDataInstance.acceleration = Mathf.CeilToInt(1.2f* PlayerDataInstance.acceleration);
                PlayerDataInstance.driftCorrection *= 1.2f;
                break;


        }
    }

    public static string UpName(Upgrades upgrade)
    {
        return upgrade switch
        {
            Upgrades.Wave => "High Yield Plasma Coils",
            Upgrades.Solidify => "Metastructural Supports",
            Upgrades.Tank => "Statishield Batteries",
            Upgrades.Surge => "Hyperefficient Generators",
            Upgrades.Overcharge => "Externalized Shield Capacitors",
            Upgrades.Lance =>"HyperNova Acceleration Lance",
            Upgrades.Juicesqueezer => "Voidwrought Accummulator",
            Upgrades.Juicedynamic => "Void Laminar Plating",
            Upgrades.Juicebox =>"Void Containment Matrix",
            Upgrades.Reload =>"Overclocked Plasma Coils",
            Upgrades.Repeater =>"Cyclic Firing Chambers",
            Upgrades.Speedboost =>"Nuetrino-Flux Propulsor",
            Upgrades.Handling =>"Gravatic Torque Amplifier",
        };
    }

    public static string UpBody(Upgrades upgrade)
    {
        return upgrade switch
        {
            Upgrades.Wave => "Increases Bullets per Shot",
            Upgrades.Solidify => "Increases Hull Integrity and Shield Capacity",
            Upgrades.Tank => "Increases Shield Capacity but decreases Shield Regeneration",
            Upgrades.Surge => "Greatly increases Shield Regenration",
            Upgrades.Overcharge => "Massively increases Shield Capacity and Regeneration but decreases Hull Integrity",
            Upgrades.Lance => "Increase Bullet Damage and Bullet Speed, Greatly decrease Rate of Fire",
            Upgrades.Juicesqueezer => "Increase Phase Shift Regeneration Speed",
            Upgrades.Juicedynamic => "Decrease Phase Shift Cost",
            Upgrades.Juicebox => "Moderately Increase Phase Shift Capacity",
            Upgrades.Reload => "Increase Rate of Fire",
            Upgrades.Repeater =>"Shoot additional Rounds of Bullets per Shot",
            Upgrades.Speedboost => "Increases Movement Speed",
            Upgrades.Handling => "Increases acceleration",
            _ => throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, null)
        };
    }
}
