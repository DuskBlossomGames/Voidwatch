using JetBrains.Annotations;
using Player;
using Spawnables.Controllers.Carcadon;
using Spawnables.Controllers.Worms;
using Spawnables.Damage;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;
using static Singletons.Static_Info.PlayerData;

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

            _rb.linearVelocity = startVel;

            var type = asteroidTypes[Random.Range(0, asteroidTypes.Length)];
            GetComponent<SpriteRenderer>().sprite = type.GetComponent<SpriteRenderer>().sprite;
            GetComponent<PolygonCollider2D>().points = type.GetComponent<PolygonCollider2D>().points;
        }

        private void Update()
        {
            var angle = Mathf.Sign(Vector3.SignedAngle(_rb.position, _rb.linearVelocity, Vector3.forward)) * 90;
            _rb.linearVelocity = Vector3.RotateTowards(_rb.linearVelocity, Quaternion.Euler(0, 0, angle) * _rb.position, 0.15f*Mathf.PI*Time.deltaTime, 0);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            
            if (!other.gameObject.TryGetComponent<Damageable>(out var dmgable)) return;
            if (!dmgable.takeAsteroidDmg) return; // don't damage other asteroids, carc, or worms

            var mult = dmgable is PlayerDamageable ? (float) PlayerDataInstance.takenCollisionDamageMult : 1;
            dmgable.Damage(mult * 0.25f * other.relativeVelocity.sqrMagnitude, gameObject, shieldMult, bleedPerc);
        }

        protected override void OnDeath(GameObject source)
        {
            base.OnDeath(source);
            if (nextSize == null || source == null) return;

            var vel = (source.GetComponent<CustomRigidbody2D>()?.linearVelocity.normalized ?? _rb.linearVelocity.normalized) * _rb.linearVelocity.magnitude;

            var vel1 = Quaternion.Euler(0, 0, -45) * vel;
            var vel2 = Quaternion.Euler(0, 0, 45) * vel;
            
            var child1 = Instantiate(nextSize, transform.position + 2*vel1.normalized, transform.rotation);
            child1.GetComponent<AsteroidController>().startVel = vel1;
            
            var child2 = Instantiate(nextSize, transform.position + 2*vel2.normalized, transform.rotation);
            child2.GetComponent<AsteroidController>().startVel = vel2;
        }
    }
}