using System;
using System.Collections.Generic;
using System.Linq;
using EnemySpawner;
using JetBrains.Annotations;
using Player;
using ProgressBars;
using Spawnables.Carcadon;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables
{
    public enum EnemyType
    {
        None,
        Mechanical,
        Organic,
        Worm,
        Carcadon,
        WormBoss
    }
    
    public class EnemyDamageable : Damageable
    {
        private const float BIT_TTL = 1;
        private const float BIT_FADE = 1.5f;
        private const float BIT_VEL = 3.5f;
        
        private const float STUN_TIME = 2;
        private const float STUN_FALL_WAIT_TIME = 5;
        private const float STUN_FALL_PERC_PER_SEC = 0.1f;

        public EnemyType enemyType; // TODO: give values for these
        
        public int maxHealth;
        public GameObject varientParent;
        
        [CanBeNull] public GameObject explosion; // only put if you want an explosion
        public Sprite[] bitOptions;
        public int numBits;
        public float explosionScale, bitScale;

        public GameObject stunBar;
        public Stunnable stunnable;
        public int hitsToStun; // 0 means can't stun

        private readonly Timer _stunTimer = new();
        private readonly Timer _stunFallWaitTimer = new();
        private readonly Timer _stunImmunityTimer = new();
        private float _stunCount;
        private ProgressBar _stunBar;
        
        // have to do this b/c unity doesn't like duplicated properties -_-
        private float _health;
        protected override float Health { get => _health; set => _health = value; }
        protected override float MaxHealth => maxHealth;

        public new void Start()
        {
            base.Start();
            Health = MaxHealth;
            
            if (stunBar != null) _stunBar = Instantiate(stunBar).GetComponent<ProgressBar>();
            _stunBar?.transform.SetParent(transform, true);

            _stunImmunityTimer.Value = 3 + Mathf.Pow(hitsToStun, 1.1f) / 4; // calculate MaxValue once
            _stunImmunityTimer.SetValue(0);
        }

        private void Update()
        {
            // TODO: actually stun/un-stun
            _stunImmunityTimer.Update();
            _stunFallWaitTimer.Update();

            if (_stunFallWaitTimer.IsFinished && _stunImmunityTimer.IsFinished && _stunTimer.IsFinished && _stunCount >= 0)
            {
                _stunCount = Mathf.Max(0, _stunCount - hitsToStun * STUN_FALL_PERC_PER_SEC * Time.deltaTime);
                _stunBar.UpdatePercentage(_stunCount, hitsToStun);
            } 

            if (!_stunTimer.IsFinished)
            {
                _stunTimer.Update();
                stunnable.UpdateStun(); 

                _stunBar.UpdatePercentage(_stunTimer.Progress, 1);
                if (_stunTimer.IsFinished)
                {
                    stunnable.UnStun();
                    _stunImmunityTimer.Value = _stunImmunityTimer.MaxValue;
                }
            }
        }

        private void OnDestroy()
        {
            if(varientParent != null) varientParent.GetComponent<EnemyVariant>().SpawnScrap(transform.position);
        }

        public void EnemyHeal(float x){ 
            _health += x;
        }

        protected override void OnDeath(GameObject source)
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
                explosionObj.GetComponent<ExplosionHandler>().PlayVisuals();
            }
        }

        public void Damage(float damage, GameObject source, List<PlayerDamageType> type)
        {
            Damage(type.Aggregate(damage, (a, t) => a * t.Modifiers[enemyType]), source);
            if (type.Contains(PlayerDamageType.Electric)) StunHit();
        }

        private void StunHit()
        {
            if (hitsToStun == 0 || !_stunTimer.IsFinished || !_stunImmunityTimer.IsFinished) return;
            _stunFallWaitTimer.Value = STUN_FALL_WAIT_TIME;
            
            _stunCount += 1;
            if (_stunCount >= hitsToStun)
            {
                _stunCount = hitsToStun; // cap it
                _stunTimer.Value = STUN_TIME;
                
                stunnable.Stun();
            }
            
            _stunBar.UpdatePercentage(_stunCount, hitsToStun);
        }
    }
}
