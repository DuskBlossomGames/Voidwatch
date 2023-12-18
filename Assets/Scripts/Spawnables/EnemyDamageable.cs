using System;
using EnemySpawner;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables
{
    public class EnemyDamageable : Damageable
    {
        [NonSerialized] public float ScrapChance;
        [NonSerialized] public int ScrapCount;
        [NonSerialized] public GameObject ScrapPrefab;
        
        public int maxHealth;
        
        // have to do this b/c unity doesn't like duplicated properties -_- 
        private float _health;
        protected override float Health { get => _health; set => _health = value; }
        protected override float MaxHealth => maxHealth;

        private new void Start()
        {
            base.Start();
            Health = maxHealth;
        }

        private void OnDestroy()
        {
            if (Random.value < ScrapChance)
            {
                for (var i = 0; i < ScrapCount; i++)
                {
                    var rigid = Instantiate(ScrapPrefab, transform.position, transform.rotation).GetComponent<CustomRigidbody2D>();
                    rigid.velocity = Random.insideUnitCircle*5;
                }
            }
        }
    }
}