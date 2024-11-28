using System;
using EnemySpawner;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables
{
    public class EnemyDamageable : Damageable
    {
        public int maxHealth;
        public GameObject varientParent;

        // have to do this b/c unity doesn't like duplicated properties -_-
        private float _health;
        protected override float Health { get => _health; set => _health = value; }
        protected override float MaxHealth => maxHealth;

        public new void Start()
        {
            base.Start();
            Health = maxHealth;
        }

        private void OnDestroy()
        {
            if(varientParent != null) varientParent.GetComponent<EnemyVariant>().SpawnScrap(transform.position);
        }

        public void EnemyHeal(float x){ 
            _health += x;
        }
    }
}
