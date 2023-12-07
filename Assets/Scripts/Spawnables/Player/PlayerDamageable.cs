using System;
using Scriptable_Objects;
using UnityEngine;

namespace Spawnables.Player
{
    public class PlayerDamageable : Damageable
    {
        public PlayerData playerData;

        protected override float MaxHealth => playerData.playerMaxHealth;
        protected override float Health
        {
            get => playerData.Health is -1 ? MaxHealth : playerData.Health;
            set => playerData.Health = value;
        }
    }
}