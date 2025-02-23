using Spawnables;
using UnityEngine;

public class WormDamageable : EnemyDamageable
{
    protected override float Health { get => _rootDamageable.health; set => _rootDamageable.health = value; }
    
    public GameObject root;
    
    private MultiDamageable _rootDamageable;
    private new void Start()
    {
        _rootDamageable = root.GetComponent<MultiDamageable>();
        base.Start();
    }
    
    protected override void OnDeath(GameObject source)
    {
        base.OnDeath(source);
        Destroy(root);
    }
}