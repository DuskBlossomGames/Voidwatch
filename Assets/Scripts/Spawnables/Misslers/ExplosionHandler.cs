using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ExplosionHandler : MonoBehaviour
{
    public AudioSource audioSource;
    // Start is called before the first frame update
    public void Run()
    {
        audioSource.Play();
        StartCoroutine(Kill(.5f));
    }

    IEnumerator Kill(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
