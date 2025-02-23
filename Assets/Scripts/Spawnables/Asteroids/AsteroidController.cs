using System;
using JetBrains.Annotations;
using Spawnables.Player;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Asteroids
{
    public class AsteroidController : EnemyDamageable
    {
        public GameObject[] asteroidTypes;
        
        [CanBeNull] public GameObject nextSize;
        
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

        public override void Damage(float damage, IDamageable.DmgType dmgType, GameObject source, float reduceMod = 1)
        {
            if (LayerMask.LayerToName(source.layer) == "Enemies") return;
            
            base.Damage(damage, dmgType, source, reduceMod);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.TryGetComponent<PlayerDamageable>(out var player)) return;

            player.Damage(100, IDamageable.DmgType.Physical, gameObject);
        }

        protected override void OnDeath(GameObject source)
        {
            // base.OnDeath(source); // spawn bits

            if (nextSize == null) return;

            var vel = source.GetComponent<CustomRigidbody2D>()?.velocity ?? _rb.velocity;

            var vel1 = Quaternion.Euler(0, 0, -45) * vel;
            var vel2 = Quaternion.Euler(0, 0, 45) * vel;
            
            var child1 = Instantiate(gameObject, transform.position + 3*vel1, transform.rotation);
            child1.GetComponent<AsteroidController>().startVel = vel1;
            
            var child2 = Instantiate(gameObject, transform.position + 3*vel2, transform.rotation);
            child1.GetComponent<AsteroidController>().startVel = vel2;
        }
    }
}