using System;
using Player;
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

        private static float GetAudioMdo(Component source)
        {
            if (source == null) return 1;
            var dist = Vector2.Distance(FindAnyObjectByType<Movement>().transform.position, source.transform.position);

            const float curviness = 0.8f;
            const float zeroDist = 150;

            var curvinessVal = Mathf.Pow(2, -1 / curviness);
            return Mathf.Clamp01(1 / (dist*(1-curvinessVal)/zeroDist + curvinessVal) - 1);
        }
        
        public static AudioSource Play(AudioClip clip, Component source, float pitch, float volume)
        {
            var obj = new GameObject("SFX - " + clip.name);
            var src = obj.AddComponent<AudioSource>();
            
            src.outputAudioMixerGroup = _instance.effects;
            src.clip = clip;
            src.pitch = pitch;
            src.volume = volume * GetAudioMdo(source);
            src.Play();
            
            obj.AddComponent<DestroyAfterAudio>();

            return src;
        }
    }
}