using JetBrains.Annotations;
using Spawnables.Damage;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Controllers.Asteroids
{
    public class AsteroidController : EnemyDamageable
    {
        public GameObject[] asteroidTypes;
        
        [CanBeNull] public GameObject nextSize;
        
        public float shieldMult, bleedPerc;
        public Vector2 startVel;
        private CustomRigidbody2D _rb;

        private LineRenderer _posVec, _velVec;
        
        private new void Start()
        {
            base.Start();
            _rb = GetComponent<CustomRigidbody2D>();

            _rb.velocity = startVel;

            var type = asteroidTypes[Random.Range(0, asteroidTypes.Length)];
            GetComponent<SpriteRenderer>().sprite = type.GetComponent<SpriteRenderer>().sprite;
            GetComponent<PolygonCollider2D>().points = type.GetComponent<PolygonCollider2D>().points;
        }

        private void Update()
        {
            var angle = Mathf.Sign(Vector3.SignedAngle(_rb.position, _rb.velocity, Vector3.forward)) * 90;
            _rb.velocity = Vector3.RotateTowards(_rb.velocity, Quaternion.Euler(0, 0, angle) * _rb.position, 0.15f*Mathf.PI*Time.deltaTime, 0);
        }

        public override void Damage(float damage, GameObject source)
        {
            if (source != null && LayerMask.LayerToName(source.layer) == "PlayerOnlyHazard") return; // prevent bifurcator from incinerating them
            
            base.Damage(damage, source);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent<Damageable>(out var dmgable)) return;
            if (other.gameObject.GetComponent<AsteroidController>() != null) return; // don't damage other asteroids
            
            dmgable.Damage(0.5f * other.relativeVelocity.sqrMagnitude, gameObject, shieldMult, bleedPerc);
        }

        protected override void OnDeath(GameObject source)
        {
            if (nextSize == null || source == null) return;

            var vel = (source.GetComponent<CustomRigidbody2D>()?.velocity.normalized ?? _rb.velocity.normalized) * _rb.velocity.magnitude;

            var vel1 = Quaternion.Euler(0, 0, -45) * vel;
            var vel2 = Quaternion.Euler(0, 0, 45) * vel;
            
            var child1 = Instantiate(nextSize, transform.position + 2*vel1.normalized, transform.rotation);
            child1.GetComponent<AsteroidController>().startVel = vel1;
            
            var child2 = Instantiate(nextSize, transform.position + 2*vel2.normalized, transform.rotation);
            child2.GetComponent<AsteroidController>().startVel = vel2;
        }
    }
}