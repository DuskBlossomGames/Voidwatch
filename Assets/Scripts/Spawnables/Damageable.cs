using UnityEngine;

namespace Spawnables
{
    public class Damageable : MonoBehaviour
    {
        private const float _koffset = 1.5f;
    
        public GameObject healthBarPrefab;
        public AnimationCurve heathOpacityCurve;
    
        protected virtual float Health { get; set; }
        protected virtual float MaxHealth { get; }
        
        private GameObject _healthBar;

        private float _barVisibility;
    
        protected void Start()
        {
            _healthBar = Instantiate(healthBarPrefab);
        }

        private void OnDestroy()
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
        
        public void Damage(float damage)
        {
            _barVisibility = 1;
            Health -= damage;
        
            if (Health <= 0)
            {
                Destroy(_healthBar);
                Destroy(gameObject);
            }
        
            // scale *2 because it extends in both directions
            Debug.Log("health perc: "+(Health/MaxHealth));
            _healthBar.transform.GetChild(0).localScale = new Vector3(
                2 * (1 - Health / MaxHealth), 1, 1);
        }
    }
}