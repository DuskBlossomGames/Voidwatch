using Spawnables;
using Spawnables.Worms;
using UnityEngine;

public class WormDamageable : EnemyDamageable
{
    protected override float MaxHealth { get => _rootDamageable.MaxHealth; }
    protected override float Health { get => _rootDamageable.Health; set => _rootDamageable.Health = value; }
    
    public GameObject root;
    public float dmgMod = 1;
    
    private HealthHolder _rootDamageable;
    
    private new void Start()
    {
        _rootDamageable = root.GetComponent<HealthHolder>();
        base.Start();
    }

    public override void Damage(float damage, GameObject source)
    {
        base.Damage(dmgMod * damage, source);
    }

    private void SpawnBits(GameObject source)
    {
        base.OnDeath(source);
    }

    protected override void OnDeath(GameObject source)
    {
        foreach (var dmg in root.GetComponentsInChildren<WormDamageable>()) dmg.SpawnBits(source);
        Destroy(root);
    }
}