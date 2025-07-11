using Bosses.Worm;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menus
{
    public class MusicTransitionManager : MonoBehaviour
    {
        private static MusicTransitionManager instance;
        public AudioSource audioPlayer;
        public AudioClip menuIntro;
        public AudioClip menuLoop;
        public float menuLoopPeak;
        public AudioClip battleIntro;
        public AudioClip battleLoop;
        public AudioClip bossIntro;
        public AudioClip bossLoop;

        public AudioSource bossOverlayPlayer;
        public AudioClip bossPhase2Intro;
        public AudioClip bossPhase2Loop;
        
        public float timeInterpolate;
        public float staticMusicVolume;

        private bool _playingIntro;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(instance);
            }
            else
            {
                Destroy(gameObject);
            }

            staticMusicVolume = audioPlayer.volume;
        }


        private void Update()
        {
            AudioClip introClip, loopClip;
            var playAt = 0f;
            var skipIntro = false;
            
            switch (SceneManager.GetActiveScene().name)
            {
                case "LevelPlay":
                {
                    introClip = battleIntro;
                    loopClip = battleLoop;
                    break;
                }
                case "LevelBoss":
                {
                    introClip = bossIntro;
                    loopClip = bossLoop;
                    
                    if (WormBrain.instance.IsStageTwo && !bossOverlayPlayer.isPlaying)
                    {
                        if (_playingIntro)
                        {
                            bossOverlayPlayer.clip = bossPhase2Intro;
                            bossOverlayPlayer.Play();
                            bossOverlayPlayer.time = audioPlayer.time;
                        }
                        else
                        {
                            bossOverlayPlayer.clip = bossPhase2Loop;
                            bossOverlayPlayer.loop = true;
                            bossOverlayPlayer.Play();
                        }
                    }

                    break;
                }
                default:
                {
                    introClip = menuIntro;
                    loopClip = menuLoop;
                    skipIntro = SceneManager.GetActiveScene().name != "SplashScreen";
                    playAt = skipIntro ? menuLoopPeak : 0;
                    
                    break;
                }
            }
            
            if (audioPlayer.clip != introClip && audioPlayer.clip != loopClip)
            {
                if (audioPlayer.isPlaying && audioPlayer.volume > 0.01f)
                {
                    audioPlayer.volume -= staticMusicVolume / timeInterpolate * Time.deltaTime;
                    audioPlayer.volume = Mathf.Clamp(audioPlayer.volume, 0f, staticMusicVolume);
                }
                else
                {
                    audioPlayer.clip = skipIntro ? loopClip : introClip;
                    audioPlayer.time = playAt;
                    audioPlayer.loop = false;
                    audioPlayer.Play();
                    _playingIntro = !skipIntro;
                }
            }
            else if (audioPlayer.volume < staticMusicVolume)
            {
                audioPlayer.volume += staticMusicVolume / timeInterpolate * Time.deltaTime;
                audioPlayer.volume = Mathf.Clamp(audioPlayer.volume, 0f, staticMusicVolume);
            }

            if (!audioPlayer.isPlaying && audioPlayer.time >= audioPlayer.clip.length && _playingIntro)
            {
                _playingIntro = false;
                audioPlayer.clip = loopClip;
                audioPlayer.loop = true;
                audioPlayer.Play();
            }
        }
    }
}