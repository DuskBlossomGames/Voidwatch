using Spawnables;
using UnityEngine;

public class ChaserDamageable : EnemyDamageable
{
    public override void Damage(float damage, GameObject source)
    {
        var cb = GetComponent<ChaserBehavior>();
        cb.TakeDamage();
        base.Damage(damage, source);
    }
}
