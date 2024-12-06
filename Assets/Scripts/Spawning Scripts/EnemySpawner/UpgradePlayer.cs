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
                PlayerDataInstance.shieldRegenRate *= 1.5f;
                break;
            case Upgrades.Overcharge: //Decrease health, decrease debt, massively increase regen and shields
                PlayerDataInstance.shieldRegenRate *= 2f;
                PlayerDataInstance.maxShield = Mathf.CeilToInt(2f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.maxShieldDebt *= .2f;
                PlayerDataInstance.maxHealth = Mathf.CeilToInt(.7f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.Health = Mathf.Min(PlayerDataInstance.Health, PlayerDataInstance.maxHealth);
                break;
        }
    }

    public static string UpName(Upgrades upgrade)
    {
        return upgrade switch
        {
            Upgrades.Wave => "High Yield Plasma Coils",
            Upgrades.Solidify => "Metastructural Supports",
            Upgrades.Tank => "Shieldgen Batteries",
            Upgrades.Surge => "Efficient Generators",
            Upgrades.Overcharge => "Externalized Shield Capacitors",
        };
    }

    public static string UpBody(Upgrades upgrade)
    {
        return upgrade switch
        {
            Upgrades.Wave => "Increases Bullets per shot",
            Upgrades.Solidify => "Increases Hull Integrity and Shield Capacity",
            Upgrades.Tank => "Increases Shield Capacity but decreases Shield Regeneration",
            Upgrades.Surge => "Greatly increases Shield Regenration",
            Upgrades.Overcharge => "Massively increases Shield Capacity and Regeneration but decreases Hull Integrity",
        };
    }
}
