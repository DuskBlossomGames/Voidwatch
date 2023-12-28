using Scriptable_Objects.Upgrades;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBaseWeapon : IBaseComponent
{
    public abstract void OnGetFocus();
    public abstract void OnLoseFocus();

    public override void HandleEvent(IUpgradeableEvent evt)
    {
        InternalHandleEvent(evt, weaponID);
    }
}
