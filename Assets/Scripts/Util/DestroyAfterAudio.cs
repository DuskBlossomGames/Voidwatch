using System;
using UnityEngine;

namespace Util
{
    public class DestroyAfterAudio : MonoBehaviour
    {
        private float _timePlaying;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
        }

        private void FixedUpdate()
        {
            if (_audioSource.isPlaying) _timePlaying += Time.fixedDeltaTime;
            if (_timePlaying > _audioSource.clip.length) Destroy(gameObject);
        }
    }
}