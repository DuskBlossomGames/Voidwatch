using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scriptable_Objects.Upgrades;

public abstract class BaseComponent : ScriptableObject
{
    public List<System.Type> eventsGenerated;
    public bool compEnabled;
    public int? weaponID;
    public string compName;
    public int compWeight;
    public BaseUpgrade.Rarity rarity;
    public enum CompType
    {
        Weapon = 1,
        Engine = 2,
        Shield = 3,
    }

    public CompType compType;

    protected Player.Upgradeable _upgradeable;

    protected void InternalHandleEvent(IUpgradeableEvent evt, int? callerWeaponID)
    {
        if (!eventsGenerated.Contains(evt.GetType())) 
            throw new Util.UndeclaredEventException(string.Format("Component doesn't declare {0} as a spawnable event type",evt.GetType()));
        _upgradeable.HandleEvent(evt, callerWeaponID);
    }

    public abstract void HandleEvent(IUpgradeableEvent evt);//Each implementation of basecomponent decides weather it needs a weaponid or not
}
