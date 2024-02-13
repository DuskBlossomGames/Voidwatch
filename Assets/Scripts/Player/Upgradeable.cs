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
        [ItemCanBeNull] private readonly List<UpgradeInstance> _upgrades = new();

        public UpgradeInstance debugAddUpgrade;
        public UpgradeInstance debugRemoveUpgrade;

        public void HandleEvent(IUpgradeableEvent evt, int? callerWeaponID)
        {
            _upgrades.ForEach(u=>u.OnEvent(this, evt, callerWeaponID));
        }
        
        private void Update()
        {
            if (debugAddUpgrade != null)
            {
                _upgrades.Add(debugAddUpgrade);
                debugAddUpgrade = null;
            }
            if (debugRemoveUpgrade != null)
            {
                _upgrades.Remove(debugRemoveUpgrade);
                debugRemoveUpgrade = null;
            }
        }
    }
}