using Scriptable_Objects;
using UnityEngine.SceneManagement;

namespace Spawnables.Player
{
    public class PlayerDamageable : ShieldDamageable
    {
        public PlayerData playerData;

        protected override float MaxHealth => playerData.playerMaxHealth;
        protected override float Health
        {
            get => playerData.Health ?? MaxHealth;
            set => playerData.Health = value;
        }

        protected override float ShieldRegenRate => playerData.playerShieldRegenRate;
        protected override float ShieldMaxPower => playerData.playerMaxShield;
        protected override float ShieldMaxDebt => playerData.playerMaxShieldDebt;

        private void OnDestroy()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}