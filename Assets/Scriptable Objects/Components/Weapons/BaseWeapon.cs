using Scriptable_Objects.Upgrades;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseWeapon : BaseComponent
{
    public virtual void Start() { }
    public virtual void Update() { }
    public abstract void OnGetFocus();
    public abstract void OnLoseFocus();

    public override void HandleEvent(IUpgradeableEvent evt)
    {
        InternalHandleEvent(evt, weaponID);
    }
}
