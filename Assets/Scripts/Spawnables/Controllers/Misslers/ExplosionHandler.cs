using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Singletons;
using Spawnables.Damage;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Controllers.Misslers
{
    public class ExplosionHandler : MonoBehaviour
    {
        public AudioClip explosionClip;
        public float shieldMult, bleedPerc;

        public GameObject source;


        public void Play(float pitch = 0.8f, float volume = 0.5f)
        {
            AudioPlayer.Play(explosionClip, this, pitch + Random.Range(-0.1f, 0.1f), volume);
            
            GetComponent<ParticleSystem>().Play();
            StartCoroutine(Kill(.5f));
        }

        public void Run(float damage, float range, GameObject source, List<Collider2D> ignore = null, int layer = -1, float enemyMod=1)
        {
            if (layer == -1) layer = source.layer;
            ignore ??= new List<Collider2D>();
            
            this.source = source;
            
            var scale = range / 5; // ~5 is the empirically derived start scale
            transform.localScale = new Vector3(scale, scale, 1);

            Play();
        
            const int rayNum = 16;
            for (var i = 0; i < rayNum; i++)
            {
                var raydir = new Vector2(Mathf.Cos(2 * Mathf.PI * i / rayNum),
                    Mathf.Sin(2 * Mathf.PI * i / rayNum));
                var hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + range * raydir, MaskUtil.COLLISION_MASKS[layer]);
                if (hit.collider == null || ignore.Contains(hit.collider)) continue;

                ignore.Add(hit.collider);
                var damageable = hit.transform.GetComponent<IDamageable>();
                damageable?.Damage((damageable is EnemyDamageable ? enemyMod : 1) * damage, gameObject, shieldMult, bleedPerc);
            }
        }

        private IEnumerator Kill(float time)
        {
            yield return new WaitForSeconds(time);
            Destroy(gameObject);
        }
    }
}
