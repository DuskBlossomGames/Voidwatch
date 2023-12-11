using Player;
using UnityEngine;

namespace Scriptable_Objects
{
    public abstract class BaseUpgrade : ScriptableObject
    {
        public enum Rarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        public new string name;
        public int weight;
        public Sprite sprite;
        public Rarity rarity;

        public virtual void Equip(Upgradeable upgradeable) { }
        public virtual void UnEquip(Upgradeable upgradeable) { }
        public virtual void Update(Upgradeable upgradeable) { }
    }
}