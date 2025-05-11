using System.Collections;
using System.Collections.Generic;
using Spawnables;
using UnityEngine;
using Util;

public class ExplosionHandler : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayVisuals()
    {
        audioSource.Play();
        GetComponent<ParticleSystem>().Play();
    }
    
    public void Run(float damage, float range, int layer, List<Collider2D> ignore)
    {
        var scale = range / 5; // ~5 is the empirically derived start scale
        transform.localScale = new Vector3(scale, 1);

        PlayVisuals();
        
        const int rayNum = 16;
        for (var i = 0; i < rayNum; i++)
        {
            var raydir = new Vector2(Mathf.Cos(2 * Mathf.PI * i / rayNum),
                Mathf.Sin(2 * Mathf.PI * i / rayNum));
            var hit = Physics2D.Linecast(transform.position, (Vector2)transform.position + range * raydir, MaskUtil.COLLISION_MASKS[layer]);
            if (hit.collider == null || ignore.Contains(hit.collider)) continue;
                    
            hit.transform.GetComponent<IDamageable>()?.Damage(damage, IDamageable.DmgType.Physical, gameObject);
        }
        StartCoroutine(Kill(.5f));
    }

    IEnumerator Kill(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
