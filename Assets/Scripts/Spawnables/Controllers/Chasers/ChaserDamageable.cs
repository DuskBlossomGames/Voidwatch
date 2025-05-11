using Spawnables;
using UnityEngine;

public class ChaserDamageable : EnemyDamageable
{
    public override void Damage(float damage, IDamageable.DmgType dmgType, GameObject source, float reduceMod = 1f)
    {
        var cb = GetComponent<ChaserBehavior>();
        cb.TakeDamage();
        base.Damage(damage, dmgType, source, reduceMod);
    }
}
