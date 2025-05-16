using System;
using Spawnables.Worms;

public class MultiDamageable : HealthHolder
{
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
        Health = maxHealth;
    }

    private void OnDestroy()
    {
        throw new NotImplementedException();
    }
}
