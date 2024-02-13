using System;
using Player;
using UnityEngine;

namespace Scriptable_Objects.Upgrades
{
    public interface IUpgradeableEvent { }

    public class ShootEvent : IUpgradeableEvent
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

    public class DealDamageEvent : IUpgradeableEvent
    {
        public GameObject damaged;
        public float damage;
    }

    public class DodgeEvent : IUpgradeableEvent
    {
        public float dodgeDistance;
        public float dodgeVelocity;
        public float dodgeCooldown;
    }

    public class MoveEvent : IUpgradeableEvent
    {
        public float acceleration;
        public float speedLimit;
    }

    // just a non-generic version
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
        public Component component;
        public bool isWeaponBased;
        public abstract void OnEvent(Upgradeable upgradeable, IUpgradeableEvent evt);
        public abstract Type GetEventType();
    }

    public abstract class BaseUpgrade<T> : BaseUpgrade where T : IUpgradeableEvent
    {        
        public override void OnEvent(Upgradeable upgradeable, IUpgradeableEvent evt)
        {
            if (evt.GetType() != typeof(T)) return;
            
            OnEvent(upgradeable, (T) evt);
        }

        public override Type GetEventType()
        {
            return typeof(T);
        }

        protected abstract void OnEvent(Upgradeable upgradeable, T evt);
    }
}