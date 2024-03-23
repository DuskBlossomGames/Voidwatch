using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spawnables;

public class ChaserDamageable : EnemyDamageable
{
    public override void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
    {
        var cb = GetComponent<ChaserBehavior>();
        cb.TakeDamage();
        base.Damage(damage, dmgType, reduceMod);
    }
}
