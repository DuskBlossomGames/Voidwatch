using System;
using UnityEngine;
using UnityEngine.Audio;
using Util;

namespace Singletons
{
    public class AudioPlayer : MonoBehaviour
    {
        private static AudioPlayer _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public AudioMixerGroup effects;
        
        public static AudioSource Play(AudioClip clip, float pitch, float volume)
        {
            var obj = new GameObject("SFX - " + clip.name);
            var src = obj.AddComponent<AudioSource>();
            
            src.outputAudioMixerGroup = _instance.effects;
            src.clip = clip;
            src.pitch = pitch;
            src.volume = volume;
            src.Play();
            
            obj.AddComponent<DestroyAfterAudio>();

            return src;
        }
    }
}