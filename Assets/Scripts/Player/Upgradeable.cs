using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Scriptable_Objects;
using UnityEngine;

namespace Player
{
    // TODO: make this stay across levels
    public class Upgradeable : MonoBehaviour
    {
        private readonly List<BaseUpgrade> _upgrades = new();
        public IReadOnlyCollection<BaseUpgrade> Upgrades => new ReadOnlyCollection<BaseUpgrade>(_upgrades.ToList());

        public BaseUpgrade debugAddUpgrade;
        public BaseUpgrade debugRemoveUpgrade;
        
        public void AddUpgrade(BaseUpgrade upgrade)
        {
            _upgrades.Add(upgrade);
            upgrade.OnEquip(this);
        }

        public bool RemoveUpgrade(BaseUpgrade upgrade)
        {
            if (_upgrades.Remove(upgrade))
            {
                upgrade.OnUnequip(this);
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (debugAddUpgrade)
            {
                AddUpgrade(debugAddUpgrade);
                debugAddUpgrade = null;
            }
            if (debugRemoveUpgrade)
            {
                RemoveUpgrade(debugRemoveUpgrade);
                debugRemoveUpgrade = null;
            }
        }
    }
}