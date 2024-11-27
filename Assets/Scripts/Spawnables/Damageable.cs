using UnityEngine;
using System.Collections.Generic;
using ProgressBars;

namespace Spawnables
{
    public class Damageable : MonoBehaviour, IDamageable
    {
        public GameObject healthBarPrefab;
        public DamageResistances dmgRes;

        protected virtual float Health { get; set; }
        protected virtual float MaxHealth { get; }
        
        protected GeneralBar _healthBar;
        
        public void Start()
        {
            dmgRes.Ready();
            _healthBar = Instantiate(healthBarPrefab).GetComponent<GeneralBar>();
            _healthBar.transform.SetParent(transform, true);
        }

        public virtual void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            damage -= reduceMod * dmgRes.dmgReduce[(int)dmgType];
            damage *= dmgRes.dmgMod[(int)dmgType];
            Health -= damage>0 ? damage : 0;
        
            if (Health <= 0)
            {
                OnDeath();
                Destroy(gameObject);
            }

            _healthBar.UpdatePercentage(Health, MaxHealth);
        }
        
        protected virtual void OnDeath() { }
    }
}