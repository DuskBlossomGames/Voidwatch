using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using LevelPlay;
using Menus;
using ProgressBars;
using Q_Vignette.Scripts;
using Spawnables;
using Spawnables.Controllers;
using Spawnables.Controllers.Bullets;
using Spawnables.Controllers.Defenses;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using Static_Info;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using static Static_Info.PlayerData;
using Random = UnityEngine.Random;
namespace Player
{
    public class PlayerDamageable : Damageable
    {
        public bool isTutorial;

        public Q_Vignette_Single vignette;

        public EnemySpawner enemySpawner;
        public GameObject fadeOut;
        public float fadeouttime = 1;

        public GameOverController gameOver;
        
        public AnimationCurve vignetteCurve;
        public float vignetteDuration, vignettePeak;
        public float sigmoidStart, sigmoidEnd;
        public float minVignetteAlpha, maxVignetteAlpha;
        public float minVignetteScale, maxVignetteScale;

        public ProgressBar healthBar, shieldBar;

        public AudioSource audioPlayer;
        public AudioClip PlayerHitShield;
        private float _AudioPlayerShieldVolumeStatic;
        private float _AudioPlayerPitchStatic;
        public AudioClip PlayerHitDamage;
        // TODO: swap PlayerHitShield and PlayerHitDamage

        public bool godmode = false;

        private const float BIT_TTL = 1;
        private const float BIT_FADE = 1.5f;
        private const float BIT_VEL = 3.5f;

        [CanBeNull] public GameObject explosion; // only put if you want an explosion
        public Sprite[] bitOptions;
        public int numBits;
        public float explosionScale, bitScale;

        protected override float MaxHealth => PlayerDataInstance.maxHealth;
        protected override float Health
        {
            get => PlayerDataInstance.Health;
            set => PlayerDataInstance.Health = value;
        }

        protected float ShieldRegenRate => PlayerDataInstance.shieldRegenRate;
        protected float ShieldMaxPower => PlayerDataInstance.maxShield;
        protected float ShieldMaxDebt => PlayerDataInstance.maxShieldDebt;
        protected float ShieldPower;

        private readonly Timer _vignetteTimer = new();
        private readonly List<float> _vignetteCacheKeys = new();
        private readonly List<float> _vignetteCacheValues = new();
        private float _vignettePeakAlpha;
        private Movement _movement;

        private Vector3 _startLoc;

        public new void Start()
        {
            _movement = GetComponent<Movement>();

            _startLoc = transform.position;

            healthBar.UpdatePercentage(Health, MaxHealth);

            Destroy(_healthBar);
            ShieldPower = ShieldMaxPower;

            for (float t = 0; t <= vignettePeak * vignetteDuration; t += Time.fixedDeltaTime)
            {
                _vignetteCacheKeys.Add(vignetteCurve.Evaluate(t / vignetteDuration));
                _vignetteCacheValues.Add(vignetteDuration - t);
            }

            _AudioPlayerPitchStatic = audioPlayer.pitch;
            _AudioPlayerShieldVolumeStatic = audioPlayer.volume;
        }

        private void Update()
        {
            // failsafe
            if (((Vector2)transform.position).sqrMagnitude > 200 * 200) Damage(float.MaxValue, FindAnyObjectByType<PointAtTargets>().gameObject);

            if (Health < 0) OnDeath(null);
            
            ShieldPower = Mathf.Clamp(ShieldPower + ShieldRegenRate * Time.deltaTime, -ShieldMaxDebt, ShieldMaxPower);
            shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
            healthBar.UpdatePercentage(Health, MaxHealth);
        }

        private void FixedUpdate()
        {
            _vignetteTimer.FixedUpdate();
            vignette.mainColor.a = _vignettePeakAlpha * vignetteCurve.Evaluate(1-_vignetteTimer.Progress);
        }

        // returns true if the shields got broken
        public bool TakeEMP(float damage)
        {
            ShieldPower = Mathf.Max(ShieldPower - damage, -ShieldMaxDebt);
            
            audioPlayer.pitch = _AudioPlayerPitchStatic + Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
            audioPlayer.volume = _AudioPlayerShieldVolumeStatic +Mathf.Log(damage)/13f; //volume of hit modulates logarithmically with damage dealth
            audioPlayer.pitch = _AudioPlayerPitchStatic -0.1f; //normal hit is static and quiet
            audioPlayer.volume = (_AudioPlayerShieldVolumeStatic );

            audioPlayer.clip = PlayerHitDamage;
            audioPlayer.Play();
            
            return ShieldPower < 0;
        }

        public override void Damage(float damage, GameObject source) { Damage(damage, source, 1, 0); }

        public override void Damage(float damage, GameObject source, float shieldMult, float bleedPerc)
        {
            if (godmode) return;

            if (PlayerDataInstance.autoDodge && damage > 10)
            {
                var cost = PlayerDataInstance.dodgeJuiceCost + Mathf.Max(PlayerDataInstance.dodgeJuiceCost/4,
                    19*Mathf.Log(damage/110.6f)); // magic formula; min cost is 5/4 normal, max dodgeable is 2000 dmg, first dmg above min cost is 200
                if (cost <= _movement.DodgeJuice)
                {
                    _movement.DodgeOnceCost = cost;
                    _movement.DodgeOnceDir = _movement.GetComponent<CustomRigidbody2D>().linearVelocity.normalized;
                    return;
                }
            }

            if (_vignetteTimer.IsFinished)
            {
                _vignetteTimer.Value = vignetteDuration;
            } else if (1 - _vignetteTimer.Progress >= vignettePeak)
            {
                var curve = vignetteCurve.Evaluate(1 - _vignetteTimer.Progress);
                for (var i = 0; i < _vignetteCacheKeys.Count; i++) // pick the closest from before peak
                {
                    if (_vignetteCacheKeys[i] > curve)
                    {
                        _vignetteTimer.SetValue(_vignetteCacheValues[i - 1]);
                        break;
                    }
                }
            }
            vignette.mainScale = (maxVignetteScale - minVignetteScale) / (1 + Mathf.Exp(-2*(damage - sigmoidStart)/(sigmoidEnd - sigmoidStart))) + minVignetteScale;
            _vignettePeakAlpha = (maxVignetteAlpha - minVignetteAlpha) / (1 + Mathf.Exp(-2*(damage - sigmoidStart)/(sigmoidEnd - sigmoidStart))) + minVignetteAlpha;

            var bleed = damage * bleedPerc;
            damage -= bleed;

            if (damage > 0)
            {
                if (ShieldPower < 0)
                {
                    bleed += damage;

                    if (damage > 5)
                    {
                        audioPlayer.pitch = _AudioPlayerPitchStatic + Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
                        audioPlayer.volume = _AudioPlayerShieldVolumeStatic +Mathf.Log(damage)/15f; //volume of hit modulates logarithmically with damage dealth

                        audioPlayer.clip = PlayerHitShield;
                        audioPlayer.Play();
                    }
                }
                else
                {
                    ShieldPower -= damage * shieldMult;
                    if (ShieldPower < -ShieldMaxDebt)
                    {
                        var overDebt = -ShieldMaxDebt - ShieldPower;
                        ShieldPower += overDebt;
                        bleed += overDebt / shieldMult;
                    }

                    audioPlayer.pitch = _AudioPlayerPitchStatic + Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
                    audioPlayer.volume = _AudioPlayerShieldVolumeStatic +Mathf.Log(damage)/13f; //volume of hit modulates logarithmically with damage dealth
                    audioPlayer.pitch = _AudioPlayerPitchStatic -0.1f; //normal hit is static and quiet
                    audioPlayer.volume = (_AudioPlayerShieldVolumeStatic );

                    audioPlayer.clip = PlayerHitDamage;
                    audioPlayer.Play();

                }
            }

            if (bleed > 0)
            {
                Health -= bleed > 0 ? bleed : 0;

                if (Health < 0)
                {
                    OnDeath(source);
                    GetComponent<SpriteRenderer>().enabled = false;
                    godmode = true;
                }
            }

            healthBar.UpdatePercentage(Health, MaxHealth);
            shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
        }

        public void Heal(float heal)
        {
            Health = Mathf.Min(Health + heal, MaxHealth);
            healthBar.UpdatePercentage(Health, MaxHealth);
        }

        private bool _died;
        protected override void OnDeath(GameObject source)
        {
            if (_died) return;
            _died = true;
            
            if (enemySpawner != null) enemySpawner.enabled = false;

            StartCoroutine(DeathFade(FindDeathInfo(source)));
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
        }

        IEnumerator DeathFade(DeathInfo diedTo)
        {
            foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;
            foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.enabled = false;
            
            _movement.SetInputBlocked(true);
            
            if (!isTutorial)
            {
                StartCoroutine(gameOver.Run(false, diedTo));
            }
            else
            {
                fadeOut.SetActive(true);
                fadeOut.GetComponent<Image>().SetAlpha(0);
                for (var i = 0; i < Mathf.RoundToInt(100 * fadeouttime); i++)
                {
                    yield return new WaitForSecondsRealtime(.01f);
                    var prog = i / (100 * fadeouttime);
                
                    fadeOut.GetComponent<Image>().SetAlpha(Mathf.Pow(prog, .5f));
                }

                transform.position = _startLoc;
                Camera.main!.transform.position = new Vector3(_startLoc.x, _startLoc.y, Camera.main.transform.position.z);
                godmode = false;
                GetComponent<CustomRigidbody2D>().linearVelocity = Vector2.zero;
                GetComponent<SpriteRenderer>().enabled = true;
                foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = true;
                foreach (var trail in GetComponentsInChildren<TrailRenderer>()) trail.enabled = true;
                _movement.SetInputBlocked(false);
                GetComponent<EnforcePlayArea>().Reset();
                Health = MaxHealth;
                ShieldPower = ShieldMaxPower;
                healthBar.UpdatePercentage(Health, MaxHealth);
                shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
                _died = false;
                yield return new WaitForSeconds(0.35f);
                fadeOut.SetActive(false);
            }
        }

        [CanBeNull]
        private static DeathInfo FindDeathInfo(GameObject source)
        {
            while (true)
            {
                if (source.TryGetComponent<BulletCollision>(out var bullet))
                {
                    source = bullet.owner;
                    continue;
                }

                if (source.TryGetComponent<MissleAim>(out var missile) && missile.owner != null)
                {
                    source = missile.owner;
                    continue;
                }

                if (source.TryGetComponent<AreaDamager>(out var ae) && ae.owner != null)
                {
                    source = ae.owner;
                    continue;
                }

                if (source.TryGetComponent<ExplosionHandler>(out var exp) && exp.source != null)
                {
                    source = exp.source;
                    continue;
                }

                return source.GetComponentInParent<DeathInfo>();
            }
        }
    }
}
