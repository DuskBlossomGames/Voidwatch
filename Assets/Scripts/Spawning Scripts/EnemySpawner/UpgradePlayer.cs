using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static void Upgrade(BulletInfo gun, Spawnables.Player.PlayerDamageable player, Upgrades upgrade)
    {
        var pdat = player.playerData;
        switch (upgrade)
        {
            case Upgrades.Wave:
                gun.bulletsPerShot = Mathf.CeilToInt(1.1f * gun.bulletsPerShot);
                gun.lateralSeperation *= 1.1f;
                break;
            case Upgrades.Solidify: //general health boost
                pdat.playerMaxHealth = Mathf.CeilToInt(1.1f * pdat.playerMaxHealth);
                pdat.playerMaxShield = Mathf.CeilToInt(1.05f * pdat.playerMaxHealth);
                break;
            case Upgrades.Tank: //Shield boost but reduce regen
                pdat.playerMaxShield = Mathf.CeilToInt(1.2f * pdat.playerMaxHealth);
                pdat.playerMaxShieldDebt *= 1.5f;
                pdat.playerShieldRegenRate *= .8f;
                break;
            case Upgrades.Surge: //Increase shield regen speed
                pdat.playerShieldRegenRate *= 1.5f;
                break;
            case Upgrades.Overcharge: //Decrease health, decrease debt, massively increase regen and shields
                pdat.playerShieldRegenRate *= 2f;
                pdat.playerMaxShield = Mathf.CeilToInt(2f * pdat.playerMaxHealth);
                pdat.playerMaxShieldDebt *= .2f;
                pdat.playerMaxHealth = Mathf.CeilToInt(.7f * pdat.playerMaxHealth);
                break;
        }
        player.playerData.playerMaxHealth *= 2;
    }
}