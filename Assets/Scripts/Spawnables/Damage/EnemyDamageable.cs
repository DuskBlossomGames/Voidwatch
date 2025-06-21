using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Player;
using ProgressBars;
using Spawnables.Controllers;
using Spawnables.Controllers.Misslers;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Util;
using Random = UnityEngine.Random;
using static Static_Info.PlayerData;

namespace Spawnables.Damage
{
    internal struct HealthPickup
    {
        public static readonly List<HealthPickup[]> HealthPickupDropsByTier = new()
        {
            new [] {new HealthPickup{Weight=0.7f, Value=0}, new HealthPickup{Weight=0.3f, Value=1}},
            new [] {new HealthPickup{Weight=1, Value=1}},
            new [] {new HealthPickup{Weight=0.5f, Value=1}, new HealthPickup{Weight=0.5f, Value=2}},
            new [] {new HealthPickup{Weight=1, Value=2}},
            new [] {new HealthPickup{Weight=1, Value=3}},
        };
        
        public float Weight;
        public float Value;
    }
    
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

        public bool dontDestroyOffscreen;
        
        [Range(1, 5)] public int tier = 1;
        public EnemyType enemyType; // TODO: give values for these
        
        public int maxHealth;
        public GameObject varientParent;

        [CanBeNull] public GameObject healthPickup; // null for no drops, regardless of tier
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
        private HealthBar _stunBar;
        
        // have to do this b/c unity doesn't like duplicated properties -_-
        private float _health;
        protected override float Health { get => _health; set => _health = value; }
        protected override float MaxHealth => maxHealth;

        public new void Start()
        {
            base.Start();
            Health = MaxHealth;
            
            if (stunBar != null) _stunBar = Instantiate(stunBar).GetComponent<HealthBar>();
            _stunBar?.transform.SetParent(transform, true);

            _stunImmunityTimer.Value = 3 + Mathf.Pow(hitsToStun, 1.1f) / 4; // calculate MaxValue once
            _stunImmunityTimer.SetValue(0);
        }

        private void Update()
        {
            if (!dontDestroyOffscreen && ((Vector2)transform.position).sqrMagnitude > 200 * 200)
            {
                Destroy(gameObject);
                return;
            }
            
            // TODO: actually stun/un-stun
            _stunImmunityTimer.Update();
            _stunFallWaitTimer.Update();

            if (hitsToStun != 0 && _stunFallWaitTimer.IsFinished && _stunImmunityTimer.IsFinished && _stunTimer.IsFinished && _stunCount >= 0)
            {
                _stunCount = Mathf.Max(0, _stunCount - hitsToStun * STUN_FALL_PERC_PER_SEC * Time.deltaTime);
                _stunBar.UpdatePercentageSilently(_stunCount, hitsToStun);
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

        public void EnemyHeal(float x){ 
            _health += x;
        }

        public void SpawnHealthPickups()
        {
            if (!PlayerDataInstance.healthPickupsEnabled || healthPickup == null) return;

            var options = HealthPickup.HealthPickupDropsByTier[tier-1];
            var sum = options.Sum(o => o.Weight);
            var pick = Random.value * sum;

            for (var i = 0; i < options.Length; i++)
            {
                if ((pick -= options[i].Weight) > 0) continue;

                for (var j = 0; j < options[i].Value; j++)
                {
                    var pickup = Instantiate(healthPickup).transform;
                    pickup.GetComponent<SpriteRenderer>().sprite = bitOptions[j];

                    var angle = 2*Mathf.PI / options[i].Value * Random.Range(i, i + 1f);
                    
                    pickup.position = transform.position + transform.lossyScale.x * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));;
                    pickup.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360f));
                    pickup.localScale = bitScale*Vector3.one;
                }
            }
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
                rb.linearVelocity = BIT_VEL * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                rb.gravityScale = 0;

                var ftd = rb.gameObject.AddComponent<SpriteFadeToDeath>();
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
                explosionObj.GetComponent<ExplosionHandler>().Play();
            }
            
            SpawnHealthPickups();
            
            if(varientParent != null) varientParent.GetComponent<EnemyVariant>().SpawnScrap(transform.position);
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
