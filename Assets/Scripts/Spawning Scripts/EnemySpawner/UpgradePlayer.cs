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
                PlayerDataInstance.maxHealth = Mathf.CeilToInt(1.1f * PlayerDataInstance.maxHealth);
                PlayerDataInstance.Health += Mathf.CeilToInt(0.1f * PlayerDataInstance.maxHealth);
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
                PlayerDataInstance.Health = Mathf.Min(PlayerDataInstance.Health.Value, PlayerDataInstance.maxHealth);
                break;
        }
    }
}