using System.Collections;
using System.Collections.Generic;
using Spawnables;
using UnityEditor;
using UnityEngine;
using Util;

public class ExplosionHandler : MonoBehaviour
{
    public AudioSource audioSource;
    public float shieldMult, bleedPerc;
    private float _AudioPlayerPitchStatic;



    public void Play()
    {
        _AudioPlayerPitchStatic = audioSource.pitch;
        audioSource.pitch = _AudioPlayerPitchStatic + Random.Range(0.1f,-0.1f); //pitch modulation for sound variance
        audioSource.Play();
        audioSource.pitch = _AudioPlayerPitchStatic;
        
        GetComponent<ParticleSystem>().Play();
        StartCoroutine(Kill(.5f));
    }

    public void Run(float damage, float range, int layer, List<Collider2D> ignore)
    {
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
            hit.transform.GetComponent<IDamageable>()?.Damage(damage, gameObject, shieldMult, bleedPerc);
        }
    }

    private IEnumerator Kill(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
