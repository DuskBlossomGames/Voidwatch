using System;
using static Singletons.Static_Info.LevelSelectData;

namespace Spawnables.Controllers.Worms
{
    public class MultiDamageable : HealthHolder
    {
        public bool scaleHardHealth; // TODO: demo only
        public float maxHealth;

        private float _health;

        public override float Health
        {
            get => _health;
            set
            {
                HealthChanged?.Invoke(_health, value);
                _health = value;
            }
        }

        public override float MaxHealth => maxHealth;

        public event Action<float, float> HealthChanged;
    
        private void Awake()
        {
            if (LevelSelectDataInstance.hardMode && scaleHardHealth) maxHealth = (int) (maxHealth*LevelSelectDataInstance.hardHealthModifier);
            Health = maxHealth;
        }
    }
}
