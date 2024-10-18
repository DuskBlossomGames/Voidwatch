using UnityEngine;
using System.Collections.Generic;

namespace Spawnables
{
    public class ShieldDamageable : MonoBehaviour, IDamageable
    {
        protected virtual float ShieldRegenRate { get; set; }
        protected virtual float ShieldPower { get; set; }
        protected virtual float ShieldMaxPower { get; set; }
        protected virtual float ShieldMaxDebt { get; set; }

        private const float _koffset = 1.5f;

        public GameObject healthBarPrefab;
        public GameObject shieldBarPrefab;
        public AnimationCurve heathOpacityCurve;

        protected virtual float Health { get; set; }
        protected virtual float MaxHealth { get; }

        private GameObject _healthBar;
        private GameObject _shieldBar;

        private float _barVisibility;
        private float _barVisibilityShield;

        public DamageResistances bodyDmgRes;
        public DamageResistances shieldDmgRes;

        private float _oldHealth = 0;
        private float _oldShield = 0;

        protected void Start()
        {
            ShieldPower = ShieldMaxPower;
            bodyDmgRes.Ready();
            shieldDmgRes.Ready();
            _healthBar = Instantiate(healthBarPrefab);
            _shieldBar = Instantiate(shieldBarPrefab);
        }

        private void OnDestroy()
        {
            Destroy(_healthBar);
        }
        private void Update()
        {
            ShieldPower = Mathf.Clamp(ShieldPower + ShieldRegenRate * Time.deltaTime, -ShieldMaxDebt, ShieldMaxPower);
        }

        private void LateUpdate()
        {
            var camAngle = -Camera.main.transform.eulerAngles.z * Mathf.Deg2Rad;

            _healthBar.transform.rotation = Camera.main.transform.rotation;
            _healthBar.transform.position =
                transform.position + new Vector3(_koffset * Mathf.Sin(camAngle), _koffset * Mathf.Cos(camAngle), 0);
            _shieldBar.transform.rotation = Camera.main.transform.rotation;
            _shieldBar.transform.position =
                transform.position + new Vector3((_koffset+.2f) * Mathf.Sin(camAngle), (_koffset + .2f) * Mathf.Cos(camAngle), 0);

            if (_oldShield != ShieldPower)
            {

                _oldShield = ShieldPower;
                _barVisibility = _barVisibilityShield = Mathf.Max(_barVisibilityShield, .9f);
                //_barVisibility = 1;
                _shieldBar.transform.GetChild(0).localScale = new Vector3(
                    (Mathf.Clamp(ShieldPower, 0, ShieldMaxPower) / ShieldMaxPower), 1, 1);
            }
            if (_oldHealth != Health)
            {
                _oldHealth = Health;
                _barVisibility = _barVisibilityShield = Mathf.Max(_barVisibilityShield, .9f);
                //_barVisibility = 1;
                _healthBar.transform.GetChild(0).localScale = new Vector3(
                    2 * (1 - Health / MaxHealth), 1, 1);
            }
            
            _barVisibility = _barVisibilityShield = _barVisibility -= .7f * Time.deltaTime;

            foreach (var sprite in _healthBar.GetComponentsInChildren<SpriteRenderer>())
            {
                var color = sprite.color;
                
                //color.a = _barVisibilityShield>0 ? 0 : heathOpacityCurve.Evaluate(_barVisibility);
                color.a = heathOpacityCurve.Evaluate(_barVisibility);

                sprite.color = color;
            }
            {
                var sprite = _shieldBar.transform.GetChild(0).GetComponent<SpriteRenderer>();
                var color = sprite.color;
                //_barVisibilityShield -= .4f * Time.deltaTime;
                color.a = heathOpacityCurve.Evaluate(_barVisibilityShield);

                sprite.color = color;
            }

        }

        public void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
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

            bleed -= reduceMod * bodyDmgRes.dmgReduce[(int)dmgType];
            bleed *= bodyDmgRes.dmgMod[(int)dmgType];

            if (bleed > 0)
            {
                Health -= bleed > 0 ? bleed : 0;

                if (Health < 0)
                {
                    this.OnDeath();
                    Destroy(_shieldBar);
                    Destroy(_healthBar);
                    Destroy(gameObject);
                }
            }

            _barVisibility = _barVisibilityShield = 1;
        }

        protected virtual void OnDeath() { }
    }
}