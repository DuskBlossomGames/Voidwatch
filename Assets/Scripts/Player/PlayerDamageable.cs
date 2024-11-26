using Scriptable_Objects;
using Static_Info;
using UnityEngine.ResourceManagement.Diagnostics;
using UnityEngine.SceneManagement;
using Util;

using static Static_Info.PlayerData;
namespace Spawnables.Player
{
    public class PlayerDamageable : ShieldDamageable
    {
        
        public bool godmode = false;

        protected override float MaxHealth => PlayerDataInstance.playerMaxHealth;
        protected override float Health
        {
            get => PlayerDataInstance.Health ?? MaxHealth;
            set
            {
                PlayerDataInstance.Health = value;
                if (value <= 0) OnDeath();
            }
        }

        protected override float ShieldRegenRate => PlayerDataInstance.playerShieldRegenRate;
        protected override float ShieldMaxPower => PlayerDataInstance.playerMaxShield;
        protected override float ShieldMaxDebt => PlayerDataInstance.playerMaxShieldDebt;

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