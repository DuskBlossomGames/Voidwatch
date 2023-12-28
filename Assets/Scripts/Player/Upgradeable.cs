using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Scriptable_Objects;
using Scriptable_Objects.Upgrades;
using UnityEngine;

namespace Player
{
    // TODO: make this stay across levels
    public class Upgradeable : MonoBehaviour
    {
        [ItemCanBeNull] private readonly List<BaseUpgrade> _upgrades = new();

        public BaseUpgrade debugAddUpgrade;
        public BaseUpgrade debugRemoveUpgrade;

        public void HandleEvent(IUpgradeableEvent evt, int? callerWeaponID)
        {
            _upgrades.ForEach(u=>u.OnEvent(this, evt, callerWeaponID));
        }
        
        private void Update()
        {
            if (debugAddUpgrade)
            {
                _upgrades.Add(debugAddUpgrade);
                debugAddUpgrade = null;
            }
            if (debugRemoveUpgrade)
            {
                _upgrades.Remove(debugRemoveUpgrade);
                debugRemoveUpgrade = null;
            }
        }
    }
}