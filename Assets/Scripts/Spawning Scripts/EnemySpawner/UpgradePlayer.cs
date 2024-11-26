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
                GunInfoInstance.bulletsPerShot = Mathf.CeilToInt(1.1f * GunInfoInstance.bulletsPerShot);
                GunInfoInstance.lateralSeperation *= 1.1f;
                break;
            case Upgrades.Solidify: //general health boost
                PlayerDataInstance.playerMaxHealth = Mathf.CeilToInt(1.1f * PlayerDataInstance.playerMaxHealth);
                PlayerDataInstance.Health += Mathf.CeilToInt(0.1f * PlayerDataInstance.playerMaxHealth);
                PlayerDataInstance.playerMaxShield = Mathf.CeilToInt(1.05f * PlayerDataInstance.playerMaxShield);
                break;
            case Upgrades.Tank: //Shield boost but reduce regen
                PlayerDataInstance.playerMaxShield = Mathf.CeilToInt(1.2f * PlayerDataInstance.playerMaxHealth);
                PlayerDataInstance.playerMaxShieldDebt *= 1.5f;
                PlayerDataInstance.playerShieldRegenRate *= .8f;
                break;
            case Upgrades.Surge: //Increase shield regen speed
                PlayerDataInstance.playerShieldRegenRate *= 1.5f;
                break;
            case Upgrades.Overcharge: //Decrease health, decrease debt, massively increase regen and shields
                PlayerDataInstance.playerShieldRegenRate *= 2f;
                PlayerDataInstance.playerMaxShield = Mathf.CeilToInt(2f * PlayerDataInstance.playerMaxHealth);
                PlayerDataInstance.playerMaxShieldDebt *= .2f;
                PlayerDataInstance.playerMaxHealth = Mathf.CeilToInt(.7f * PlayerDataInstance.playerMaxHealth);
                PlayerDataInstance.Health = Mathf.Min(PlayerDataInstance.Health.Value, PlayerDataInstance.playerMaxHealth);
                break;
        }
    }
}