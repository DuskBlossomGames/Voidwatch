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

        public struct ShootInfo
        {
            public int bulletsPerShot;
            public int bulletsPerShotVarience;
            public float shotForce;
            public float forceVarience;
            public float lateralSeperation;
            public float verticalSeperation;
            public float misfireChance;
            public int repeats;
            public float repeatSeperation;
        }

        public new string name;
        public int weight;
        public Sprite sprite;
        public Rarity rarity;

        public virtual void OnEquip(Upgradeable upgradeable) { }
        public virtual void OnUnequip(Upgradeable upgradeable) { }
        public virtual float OnDealDamage(GameObject other, float damage) { return damage; }
        public virtual void OnShoot(ShootInfo info) { }
    }
}