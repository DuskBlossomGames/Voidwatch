using UnityEngine;
using System.Collections.Generic;

namespace Spawnables
{
    public class Damageable : MonoBehaviour, IDamageable
    {
        private const float _koffset = 1.5f;
    
        public GameObject healthBarPrefab;
        public AnimationCurve heathOpacityCurve;
    
        protected virtual float Health { get; set; }
        protected virtual float MaxHealth { get; }
        
        protected GameObject _healthBar;

        private float _barVisibility;

        public DamageResistances dmgRes;
    
        protected void Start()
        {
            dmgRes.Ready();
            _healthBar = Instantiate(healthBarPrefab);
        }

        protected void OnDestroy()
        {
            Destroy(_healthBar);
        }

        private void LateUpdate()
        {
            var camAngle = -Camera.main.transform.eulerAngles.z * Mathf.Deg2Rad;

            _healthBar.transform.rotation = Camera.main.transform.rotation;
            _healthBar.transform.position = 
                transform.position + new Vector3(_koffset*Mathf.Sin(camAngle), _koffset*Mathf.Cos(camAngle), 0);
        
            foreach (var sprite in _healthBar.GetComponentsInChildren<SpriteRenderer>())
            {
                var color = sprite.color;
                _barVisibility -= .4f * Time.deltaTime;
                color.a = heathOpacityCurve.Evaluate(_barVisibility);
            
                sprite.color = color;
            }
        }

        public void Damage(float damage, IDamageable.DmgType dmgType, float reduceMod = 1f)
        {
            damage -= reduceMod * dmgRes.dmgReduce[(int)dmgType];
            damage *= dmgRes.dmgMod[(int)dmgType];
            _barVisibility = 1;
            Health -= damage>0 ? damage : 0;
        
            if (Health <= 0)
            {
                Destroy(_healthBar);
                Destroy(gameObject);
            }
        
            // scale *2 because it extends in both directions
            _healthBar.transform.GetChild(0).localScale = new Vector3(
                2 * (1 - Health / MaxHealth), 1, 1);
        }
    }
}