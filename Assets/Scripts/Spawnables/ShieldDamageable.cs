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

        public DamageResistances bodyDmgRes;
        public DamageResistances shieldDmgRes;

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
                transform.position + new Vector3(_koffset * Mathf.Sin(camAngle), _koffset * Mathf.Cos(camAngle), 0);

            foreach (var sprite in _healthBar.GetComponentsInChildren<SpriteRenderer>())
            {
                var color = sprite.color;
                _barVisibility -= .4f * Time.deltaTime;
                color.a = heathOpacityCurve.Evaluate(_barVisibility);

                sprite.color = color;
            }
            foreach (var sprite in _shieldBar.GetComponentsInChildren<SpriteRenderer>())
            {
                var color = sprite.color;
                _barVisibility -= .4f * Time.deltaTime;
                color.a = heathOpacityCurve.Evaluate(_barVisibility);

                sprite.color = color;
            }
        }

        public void Damage(float damage, IDamageable.DmgType dmgType)
        {
            Debug.LogFormat("Took {0} dmg as type {1}", damage, dmgType);
            float bleed = damage * shieldDmgRes.dmgBleed[(int)dmgType];
            damage -= bleed;//some damage leaks through

            Debug.LogFormat("Multiplied by {0}", shieldDmgRes.dmgMod[(int)dmgType]);
            damage *= shieldDmgRes.dmgMod[(int)dmgType];
            Debug.LogFormat("Reduced by {0}", shieldDmgRes.dmgReduce[(int)dmgType]);
            damage -= shieldDmgRes.dmgReduce[(int)dmgType];
            Debug.LogFormat("Giving {0}", damage);
            if (damage > 0)
            {
                Debug.Log(">0");
                if (ShieldPower < 0)
                {
                    bleed += damage;
                }
                else
                {
                    Debug.LogFormat("Causes harm @ {0}",damage);
                    Debug.LogFormat("Current Power: {0}",ShieldPower);
                    ShieldPower -= damage;
                    ShieldPower -= 1;
                    Debug.LogFormat("After Power: {0}", ShieldPower);
                    if (ShieldPower < -ShieldMaxDebt)
                    { 
                        float overDebt = -ShieldMaxDebt - ShieldPower;
                        ShieldPower += overDebt;
                        bleed += overDebt;//excess damage overflows to bleeded damage
                        Debug.Log("Dmg Overflow");
                    }
                }
            }

            bleed *= bodyDmgRes.dmgMod[(int)dmgType];
            bleed -= bodyDmgRes.dmgReduce[(int)dmgType];

            _barVisibility = 1;
            Health -= bleed;

            if (Health <= 0)
            {
                Destroy(_healthBar);
                Destroy(gameObject);
            }

            _healthBar.transform.GetChild(0).localScale = new Vector3(
                2 * (1 - Health / MaxHealth), 1, 1);
            _shieldBar.transform.GetChild(0).localScale = new Vector3(
                2 * (1 - Mathf.Clamp(ShieldPower,0,ShieldMaxPower) / ShieldMaxPower), 1, 1);
        }
    }
}