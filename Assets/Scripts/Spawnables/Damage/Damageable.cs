using ProgressBars;
using UnityEngine;

namespace Spawnables.Damage
{
    public class Damageable : MonoBehaviour, IDamageable
    {
        public GameObject healthBarPrefab;

        public bool takeAsteroidDmg = true;
        public bool IsDead => Health <= 0;
        
        public virtual float Health { get; set; }
        public virtual float MaxHealth { get; }
        
        protected ProgressBar _healthBar;
        
        public void Start()
        {
            _healthBar = healthBarPrefab == null ? null : Instantiate(healthBarPrefab).GetComponent<ProgressBar>();
            _healthBar?.transform.SetParent(transform, true);
        }
        
        public virtual bool Damage(float damage, GameObject source)
        {
            if (!enabled || IsDead) return false;
            
            Health -= damage>0 ? damage : 0;
        
            if (IsDead)
            {
                OnDeath(source);
                Destroy(gameObject);
            }

            _healthBar?.UpdatePercentage(Health, MaxHealth);

            return true;
        }
        
        public virtual bool Damage(float damage, GameObject source, float shieldMult, float bleedPerc) { return Damage(damage, source); }
        
        protected virtual void OnDeath(GameObject source) { }
    }
}