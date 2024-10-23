using Scriptable_Objects;
using UnityEngine.ResourceManagement.Diagnostics;
using UnityEngine.SceneManagement;

namespace Spawnables.Player
{
    public class PlayerDamageable : ShieldDamageable
    {
        public PlayerData playerData;
        public bool godmode = false;

        protected override float MaxHealth => playerData.playerMaxHealth;
        protected override float Health
        {
            get => playerData.Health ?? MaxHealth;
            set
            {
                playerData.Health = value;
                if (value <= 0) OnDeath();
            }
        }

        protected override float ShieldRegenRate => playerData.playerShieldRegenRate;
        protected override float ShieldMaxPower => playerData.playerMaxShield;
        protected override float ShieldMaxDebt => playerData.playerMaxShieldDebt;

        public override void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            if (godmode) return;
            base.Damage(damage, dmgType, reduceMod);
        }

        protected override void OnDeath()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}