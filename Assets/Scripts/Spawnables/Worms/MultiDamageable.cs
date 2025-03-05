using System;
using Spawnables.Worms;
using UnityEngine;
using UnityEngine.Serialization;

public class MultiDamageable : HealthHolder
{
    public float maxHealth;
    
    public override float Health { get; set; }
    public override float MaxHealth => maxHealth;

    private void Awake()
    {
        Health = maxHealth;
    }
}
