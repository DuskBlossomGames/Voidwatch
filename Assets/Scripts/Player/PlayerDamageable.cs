using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using JetBrains.Annotations;
using ProgressBars;
using Q_Vignette.Scripts;
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
       
        public GameObject fadeOut;
        public float fadeouttime = 1;
        public TextMeshProUGUI title, subtitle;
        public GameObject[] statistics;
        public float statisticsBeforeTime, statisticsFadeTime, statisticsBetweenTime; 
        
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
            set
            {
                PlayerDataInstance.Health = value;
                if (value <= 0) OnDeath(null);
            }
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
            if (((Vector2)transform.position).sqrMagnitude > 200 * 200) Damage(float.MaxValue, null);
            
            ShieldPower = Mathf.Clamp(ShieldPower + ShieldRegenRate * Time.deltaTime, -ShieldMaxDebt, ShieldMaxPower);
            shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
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

            return ShieldPower < 0;
        }

        public override void Damage(float damage, GameObject source) { Damage(damage, source, 1, 0); }

        public override void Damage(float damage, GameObject source, float shieldMult, float bleedPerc)
        {
            if (godmode) return;

            if (PlayerDataInstance.autoDodge)
            {
                var cost = PlayerDataInstance.dodgeJuiceCost + Mathf.Max(PlayerDataInstance.dodgeJuiceCost/4,
                    19*Mathf.Log(damage/110.6f)); // magic formula; max dodgeable is 2000 dmg, first dmg above min cost is 200
                if (cost <= _movement.DodgeJuice)
                {
                    _movement.DodgeOnceCost = cost;
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

        protected override void OnDeath(GameObject source)
        {

            StartCoroutine(DeathFade());
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

        IEnumerator DeathFade()
        {
            for (var i = 0; i < transform.childCount; i++) if (transform.GetChild(i).GetComponent<ParticleSystem>() == null) transform.GetChild(i).gameObject.SetActive(false);
            
            _movement.SetInputBlocked(true);
            fadeOut.SetActive(true);
            title.gameObject.SetActive(true);
            subtitle.gameObject.SetActive(true);
            fadeOut.GetComponent<Image>().SetAlpha(0);
            title.SetAlpha(0);
            subtitle.SetAlpha(0);
            for (var i = 0; i < Mathf.RoundToInt(100 * fadeouttime); i++)
            {
                yield return new WaitForSecondsRealtime(.01f);
                var prog = i / (100 * fadeouttime);
                fadeOut.GetComponent<Image>().SetAlpha(Mathf.Pow(prog, .5f));
                title.SetAlpha(Mathf.Pow(prog, .5f));
                subtitle.SetAlpha(Mathf.Pow(prog, .5f));
            }

            if (!isTutorial)
            {
                yield return new WaitForSeconds(statisticsBeforeTime);
                Statistics.SetText(statistics.Select(o => o.GetComponentsInChildren<TextMeshProUGUI>()[1]).ToArray());
                
                foreach (var stat in statistics)
                {
                    stat.SetActive(true);
                    for (float t = 0; t < statisticsFadeTime; t += Time.fixedDeltaTime)
                    {
                        stat.GetComponentInChildren<Image>().SetAlpha(t/statisticsFadeTime);
                        foreach (var text in stat.GetComponentsInChildren<TextMeshProUGUI>()) text.SetAlpha(t/statisticsFadeTime);
                        
                        yield return new WaitForFixedUpdate();
                    }
                    
                    yield return new WaitForSeconds(statisticsBetweenTime);
                }
                
                while (!InputManager.GetKeyDown(KeyCode.Return) && !InputManager.GetKeyDown(KeyCode.Escape) &&
                       !InputManager.GetKeyDown(KeyCode.Mouse0) && !InputManager.GetKeyDown(KeyCode.Space))
                {
                    yield return new WaitForFixedUpdate();
                }
                
                Destroy(StaticInfoHolder.Instance.gameObject); // reset static info
                SceneManager.LoadScene("TitleScreen"); // return to boot
            }
            else
            {
                transform.position = _startLoc;
                godmode = false;
                GetComponent<CustomRigidbody2D>().linearVelocity = Vector2.zero;
                GetComponent<SpriteRenderer>().enabled = true;
                for (var i = 0; i < transform.childCount; i++) if (transform.GetChild(i).GetComponent<ParticleSystem>() == null) transform.GetChild(i).gameObject.SetActive(true);
                _movement.SetInputBlocked(false);
                fadeOut.SetActive(false);
                Heal(MaxHealth);
            }
        }
    }
}
