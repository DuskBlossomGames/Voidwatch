using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scriptable_Objects.Upgrades;

public class UpgradeInstance
{
    public BaseUpgrade upgrade;
    public bool enabled;
    public int? weaponID;


    public void OnEvent(Player.Upgradeable upgradeable, IUpgradeableEvent evt, int? callerWeaponID)
    {
        if (callerWeaponID != null && callerWeaponID != weaponID) return;
        if (!enabled) return;

        upgrade.OnEvent(upgradeable, evt);
    }
}
