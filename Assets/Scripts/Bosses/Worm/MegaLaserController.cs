using System;
using System.Collections.Generic;
using Spawnables;
using UnityEngine;
using Util;

namespace Bosses.Worm
{
    public class MegaLaserController : MonoBehaviour
    {
        public float timeToLive, fadeTime, damage;

        private BoxCollider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private readonly Timer _aliveTimer = new();
        private readonly Timer _fadeTimer = new();
        
        private void Start()
        {
            _collider = gameObject.AddComponent<BoxCollider2D>();

            _aliveTimer.Value = timeToLive;
            _fadeTimer.Value = fadeTime;
        }

        private void Update()
        {
            var parent = transform.parent;
            var parentScale = parent == null ? new Vector3(1, 1, 1) : parent.localScale;

            transform.localRotation = Quaternion.identity;
            // transform.localScale = new Vector2(length / parentScale.x, width / parentScale.y);
            // transform.localPosition = new Vector2(length / 2 / parentScale.x, 0);
            
            _aliveTimer.Update();
            if (_aliveTimer.IsFinished) _fadeTimer.Update();

            if (_aliveTimer.IsActive)
            {
                var colliding = new List<Collider2D>();
                _collider.OverlapCollider(new ContactFilter2D().NoFilter(), colliding);
            
                foreach (var coll in colliding)
                {
                    coll.gameObject.GetComponent<IDamageable>()?
                        .Damage(damage, IDamageable.DmgType.Energy);
                }
            }

            _spriteRenderer.color =
                new Color(255, 0, 255, _fadeTimer.Progress);
            
            if (_fadeTimer.IsFinished) Destroy(gameObject);
        }
    }
}