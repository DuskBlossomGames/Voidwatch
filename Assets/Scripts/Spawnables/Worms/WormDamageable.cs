using Spawnables;
using Spawnables.Worms;
using UnityEngine;
using UnityEngine.Serialization;

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

    public override void Damage(float damage, IDamageable.DmgType dmgType, GameObject source, float reduceMod = 1)
    {
        base.Damage(dmgMod*damage, dmgType, source, reduceMod);
        print("damaged (now have "+Health+"/"+MaxHealth+")");
    }

    protected override void OnDeath(GameObject source)
    {
        base.OnDeath(source);
        Destroy(root);
    }
}