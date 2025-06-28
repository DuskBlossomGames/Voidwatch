using ProgressBars;
using UnityEngine;

namespace Spawnables.Damage
{
    public class Damageable : MonoBehaviour, IDamageable
    {
        public GameObject healthBarPrefab;

        public bool takeAsteroidDmg = true;
        public bool IsDead => Health <= 0;
        
        protected virtual float Health { get; set; }
        protected virtual float MaxHealth { get; }
        
        protected ProgressBar _healthBar;
        
        public void Start()
        {
            _healthBar = healthBarPrefab == null ? null : Instantiate(healthBarPrefab).GetComponent<ProgressBar>();
            _healthBar?.transform.SetParent(transform, true);
        }

        public virtual void Damage(float damage, GameObject source)
        {
            if (!enabled || IsDead) return;
            
            Health -= damage>0 ? damage : 0;
        
            if (IsDead)
            {
                OnDeath(source);
                Destroy(gameObject);
            }

            _healthBar?.UpdatePercentage(Health, MaxHealth);
        }
        
        public virtual void Damage(float damage, GameObject source, float shieldMult, float bleedPerc) { Damage(damage, source); }
        
        protected virtual void OnDeath(GameObject source) { }
    }
}