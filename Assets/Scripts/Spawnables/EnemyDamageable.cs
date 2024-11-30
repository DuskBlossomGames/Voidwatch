using System;
using EnemySpawner;
using JetBrains.Annotations;
using Spawnables.Carcadon;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables
{
    public class EnemyDamageable : Damageable
    {
        private const float BIT_TTL = 1;
        private const float BIT_FADE = 1.5f;
        private const float BIT_VEL = 3.5f;

        public int maxHealth;
        public GameObject varientParent;
        
        [CanBeNull] public GameObject explosion; // only put if you want an explosion
        public Sprite[] bitOptions;
        public int numBits;
        public float explosionScale, bitScale;

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

        protected override void OnDeath()
        {
            var angleOffset = Random.Range(0, 360f);
            for (var i = 0; i < numBits; i++)
            {
                var sprite = bitOptions[i < bitOptions.Length ? i : Random.Range(0, bitOptions.Length)];
                
                var sr = new GameObject().AddComponent<SpriteRenderer>();
                sr.sprite = sprite;

                var rb = sr.gameObject.AddComponent<Rigidbody2D>();
                var angle = 2*Mathf.PI / numBits * Random.Range(i, i + 1f) + angleOffset;
                rb.velocity = BIT_VEL * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                rb.gravityScale = 0;

                var ftd = rb.gameObject.AddComponent<FadeToDeath>();
                ftd.fadeTime = BIT_FADE;
                ftd.TimeToLive = BIT_TTL;

                ftd.transform.position = transform.position;
                ftd.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
                ftd.transform.localScale = new Vector2(bitScale, bitScale);
            }

            if (explosion != null)
            {
                var explosionObj = Instantiate(explosion, null, true);
                explosionObj.SetActive(true);
                explosionObj.transform.position = transform.position;
                explosionObj.transform.localScale = explosionScale * Vector3.one;
                explosionObj.GetComponent<ExplosionHandler>().Run();
                explosionObj.GetComponent<ParticleSystem>().Play();
            }
        }
    }
}
