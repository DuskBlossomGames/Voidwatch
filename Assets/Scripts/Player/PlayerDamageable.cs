using System;
using System.Collections.Generic;
using System.Linq;
using ProgressBars;
using Scriptable_Objects;
using Static_Info;
using UnityEngine;
using UnityEngine.ResourceManagement.Diagnostics;
using UnityEngine.SceneManagement;
using Util;

using static Static_Info.PlayerData;
namespace Spawnables.Player
{
    public class PlayerDamageable : Damageable
    {
        public Q_Vignette_Single vignette;
        public AnimationCurve vignetteCurve;
        public float vignetteDuration, vignettePeak;
        public float sigmoidStart, sigmoidEnd;
        public float minVignetteAlpha, maxVignetteAlpha;
        public float minVignetteScale, maxVignetteScale;

        public ProgressBar healthBar, shieldBar;
        public DamageResistances shieldDmgRes;

        public AudioSource audioPlayer;
        public AudioClip PlayerHitShield;
        private float _AudioPlayerShieldVolumeStatic;
        private float _AudioPlayerPitchStatic;
        public AudioClip PlayerHitDamage;

        public bool godmode = false;

        protected override float MaxHealth => PlayerDataInstance.maxHealth;
        protected override float Health
        {
            get {return PlayerDataInstance.Health; }
            set
            {
                PlayerDataInstance.Health = value;
                if (value <= 0) OnDeath();
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

        public new void Start()
        {
            healthBar.UpdatePercentage(Health, MaxHealth);

            shieldDmgRes.Ready();
            base.Start();
            Destroy(_healthBar);
            ShieldPower = ShieldMaxPower;

            for (float t = 0; t <= vignettePeak * vignetteDuration; t += Time.fixedDeltaTime)
            {
                _vignetteCacheKeys.Add(vignetteCurve.Evaluate(t / vignetteDuration));
                _vignetteCacheValues.Add(vignetteDuration - t);
            }

            _AudioPlayerPitchStatic = audioPlayer.volume;
            _AudioPlayerShieldVolumeStatic = audioPlayer.volume;
        }

        private void Update()
        {

            ShieldPower = Mathf.Clamp(ShieldPower + ShieldRegenRate * Time.deltaTime, -ShieldMaxDebt, ShieldMaxPower);
            shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
        }

        private void FixedUpdate()
        {
            _vignetteTimer.FixedUpdate();
            vignette.mainColor.a = _vignettePeakAlpha * vignetteCurve.Evaluate(1-_vignetteTimer.Progress);
        }

        public override void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            Damage(damage, 0, dmgType, reduceMod);
        }

        public void Damage(float damage, float trueDamage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            if (godmode) return;

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

            float bleed = damage * shieldDmgRes.dmgBleed[(int)dmgType];
            damage -= bleed;//some damage leaks through

            if (damage > 0)
            {
                if (ShieldPower < 0)
                {
                    bleed += damage;

                  /*  audioPlayer.pitch = _AudioPlayerPitchStatic + UnityEngine.Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
                    audioPlayer.volume = _AudioPlayerShieldVolumeStatic +Mathf.Log(damage)/15f; //volume of hit modulates logarithmically with damage dealth

                  audioPlayer.PlayOneShot(PlayerHitShield);*/

                  audioPlayer.pitch = _AudioPlayerPitchStatic + UnityEngine.Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
                  audioPlayer.volume = _AudioPlayerShieldVolumeStatic +Mathf.Log(damage)/15f; //volume of hit modulates logarithmically with damage dealth

                    audioPlayer.clip = PlayerHitShield;
                    if(damage > 5){
                      audioPlayer.Play();}
                    Debug.Log("AUDIO PLAYED!!");
                }
                else
                {
                    // only apply shield dmg res if it goes to shield
                    damage -= reduceMod * shieldDmgRes.dmgReduce[(int)dmgType];
                    damage *= shieldDmgRes.dmgMod[(int)dmgType];

                    ShieldPower -= damage;
                    ShieldPower -= 1;
                    if (ShieldPower < -ShieldMaxDebt)
                    {
                        float overDebt = -ShieldMaxDebt - ShieldPower;
                        ShieldPower += overDebt;
                        bleed += overDebt;//excess damage overflows to bleeded damage
                    }

                    bleed += trueDamage;

                    audioPlayer.pitch = _AudioPlayerPitchStatic + UnityEngine.Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
                    audioPlayer.volume = _AudioPlayerShieldVolumeStatic +Mathf.Log(damage)/13f; //volume of hit modulates logarithmically with damage dealth
                    audioPlayer.pitch = _AudioPlayerPitchStatic -0.1f; //normal hit is static and quiet
                    audioPlayer.volume = (_AudioPlayerShieldVolumeStatic );

                    audioPlayer.clip = PlayerHitDamage;
                    audioPlayer.Play();

                }
            }
            else
            {
                bleed += trueDamage;
            }

            bleed -= reduceMod * dmgRes.dmgReduce[(int)dmgType];
            bleed *= dmgRes.dmgMod[(int)dmgType];

            if (bleed > 0)
            {
                Health -= bleed > 0 ? bleed : 0;

                if (Health < 0)
                {
                    OnDeath();
                    Destroy(gameObject);
                }
            }

            healthBar.UpdatePercentage(Health, MaxHealth);
            shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
        }

        protected override void OnDeath()
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
