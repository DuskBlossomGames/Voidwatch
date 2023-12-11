using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Scriptable_Objects;
using UnityEngine;

namespace Player
{
    public class Upgradeable : MonoBehaviour
    {
        private readonly List<BaseUpgrade> _upgrades = new();
        public IReadOnlyCollection<BaseUpgrade> Upgrades => new ReadOnlyCollection<BaseUpgrade>(_upgrades.ToList());

        public BaseUpgrade debugAddUpgrade;
        public BaseUpgrade debugRemoveUpgrade;
        
        public void AddUpgrade(BaseUpgrade upgrade)
        {
            _upgrades.Add(upgrade);
            upgrade.Equip(this);
        }

        public bool RemoveUpgrade(BaseUpgrade upgrade)
        {
            if (_upgrades.Remove(upgrade))
            {
                upgrade.UnEquip(this);
                return true;
            }

            return false;
        }

        private void Update()
        {
            _upgrades.ForEach(u => u.Update(this));

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