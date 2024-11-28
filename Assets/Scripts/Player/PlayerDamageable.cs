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
        
        public ProgressBar healthBar, shieldBar;
        public DamageResistances shieldDmgRes;
        
        public bool godmode = false;

        protected override float MaxHealth => PlayerDataInstance.maxHealth;
        protected override float Health
        {
            get => PlayerDataInstance.Health ?? MaxHealth;
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
        
        public new void Start()
        {
            base.Start();
            Destroy(_healthBar);
            ShieldPower = ShieldMaxPower;

            for (float t = 0; t <= vignettePeak * vignetteDuration; t += Time.fixedDeltaTime)
            {
                var alpha = vignetteCurve.Evaluate(t / vignetteDuration);
                _vignetteCacheKeys.Add(alpha);
                _vignetteCacheValues.Add(vignetteDuration - t);
            }
        }
        
        private void Update()
        {
            ShieldPower = Mathf.Clamp(ShieldPower + ShieldRegenRate * Time.deltaTime, -ShieldMaxDebt, ShieldMaxPower);
            shieldBar.UpdatePercentage(ShieldPower, ShieldMaxPower);
        }

        private void FixedUpdate()
        {
            _vignetteTimer.FixedUpdate();
            vignette.mainColor.a = vignetteCurve.Evaluate(1-_vignetteTimer.Progress);
        }

        public override void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            if (godmode) return;

            if (_vignetteTimer.IsFinished)
            {
                _vignetteTimer.Value = vignetteDuration;
            } else if (1 - _vignetteTimer.Progress >= vignettePeak)
            {
                for (var i = 0; i < _vignetteCacheKeys.Count; i++) // pick the closest from before peak
                {
                    if (_vignetteCacheKeys[i] > vignette.mainColor.a)
                    {
                        _vignetteTimer.SetValue(_vignetteCacheValues[i - 1]);
                        break;
                    }
                }

            }
            
            float bleed = damage * shieldDmgRes.dmgBleed[(int)dmgType];
            damage -= bleed;//some damage leaks through

            damage -= reduceMod * shieldDmgRes.dmgReduce[(int)dmgType];
            damage *= shieldDmgRes.dmgMod[(int)dmgType];
            
            if (damage > 0)
            {
                if (ShieldPower < 0)
                {
                    bleed += damage;
                }
                else
                {
                    ShieldPower -= damage;
                    ShieldPower -= 1;
                    if (ShieldPower < -ShieldMaxDebt)
                    { 
                        float overDebt = -ShieldMaxDebt - ShieldPower;
                        ShieldPower += overDebt;
                        bleed += overDebt;//excess damage overflows to bleeded damage
                    }
                }
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