using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bosses.Worm;

public class MusicTransitionManager : MonoBehaviour
{

    private static MusicTransitionManager instance;
    public AudioSource audioPlayer;
    public AudioClip menuIntro;
    public AudioClip menuLoop;
    public AudioClip battleIntro;
    public AudioClip battleLoop;
    public AudioClip bossIntro;
    public AudioClip bossLoop;

    public AudioSource bossOverlayPlayer;
    public AudioClip bossPhase2Intro;
    public AudioClip bossPhase2Loop;

    private bool _playingIntro;

    /*public Scene levelSelect;
    public Scene levelPlay;
    public Scene levelBoss;*/

    public float timeInterpolate;
    public float staticMusicVolume;

    // Start is called before the first frame update
    void Awake(){
      if(instance==null){
        instance = this;
        DontDestroyOnLoad(instance);
      } else {
          Destroy(gameObject);

      }

      staticMusicVolume = audioPlayer.volume;

    }


    void Update(){
      if(SceneManager.GetActiveScene().name == "LevelPlay"){

        print("I am in level play");

          if(audioPlayer.clip != battleIntro && audioPlayer.clip != battleLoop){

              print("I am in level play and it is playing the wrong thing!");


            if(audioPlayer.isPlaying && audioPlayer.volume > 0.01f){

              audioPlayer.volume -= 1/timeInterpolate * Time.deltaTime;
              Mathf.Clamp(audioPlayer.volume,0f,staticMusicVolume);
              print("Curret volume " + audioPlayer.volume);

            } else{

              audioPlayer.clip = battleIntro;
              audioPlayer.Play();
              _playingIntro = true;
              audioPlayer.Play();

            }

          }

          if(audioPlayer.clip == battleIntro && audioPlayer.volume < staticMusicVolume){
            audioPlayer.volume += 1/timeInterpolate * Time.deltaTime;
            Mathf.Clamp(audioPlayer.volume,0f,staticMusicVolume);

          }

          if(!audioPlayer.isPlaying && _playingIntro){
            _playingIntro = false;
            audioPlayer.clip = battleLoop;
            audioPlayer.loop = true;
          }


      } else if(SceneManager.GetActiveScene().name == "LevelBoss"){

          if(audioPlayer.clip != bossIntro && audioPlayer.clip != bossLoop){

            if(audioPlayer.isPlaying && audioPlayer.volume > 0.01f){

              audioPlayer.volume -= 1/timeInterpolate * Time.deltaTime;
              Mathf.Clamp(audioPlayer.volume,0f,staticMusicVolume);

            } else{

              audioPlayer.clip = bossIntro;
              audioPlayer.Play();
              _playingIntro = true;

            }

          }

          if(audioPlayer.clip == bossIntro && audioPlayer.volume < staticMusicVolume){
            audioPlayer.volume += 1/timeInterpolate * Time.deltaTime;
            Mathf.Clamp(audioPlayer.volume,0f,staticMusicVolume);

          }

          if(!audioPlayer.isPlaying && _playingIntro){
            _playingIntro = false;
            audioPlayer.clip = bossLoop;
            audioPlayer.loop = true;
            audioPlayer.Play();
          }

          if(WormBrain.instance.IsStageTwo && !bossOverlayPlayer.isPlaying){

            if(_playingIntro){
              bossOverlayPlayer.clip = bossPhase2Intro;
              bossOverlayPlayer.Play();
              bossOverlayPlayer.time = audioPlayer.time;

          } else{
              bossOverlayPlayer.clip = bossPhase2Loop;
              bossOverlayPlayer.loop = true;
              bossOverlayPlayer.Play();
            }

          }




      } else{

        print("I am in level select" + audioPlayer.clip);

        if(audioPlayer.clip != menuIntro && audioPlayer.clip != menuLoop){
            print("I am in level select and it is not playing the right clips");

          if(audioPlayer.isPlaying && audioPlayer.volume > 0.01f){

            audioPlayer.volume -= 1/timeInterpolate * Time.deltaTime;
            Mathf.Clamp(audioPlayer.volume,0f,staticMusicVolume);

            //print("big audio " + audioPlayer.volume);

          } else{

            audioPlayer.clip = menuIntro;
            audioPlayer.Play();
            _playingIntro = true;

          }

        }

        if(audioPlayer.clip == menuIntro && audioPlayer.volume < staticMusicVolume){
          audioPlayer.volume += 1/timeInterpolate * Time.deltaTime;
          Mathf.Clamp(audioPlayer.volume,0f,staticMusicVolume);

        }

        if(!audioPlayer.isPlaying && _playingIntro){
          _playingIntro = false;
          audioPlayer.clip = menuLoop;
          audioPlayer.loop = true;
          audioPlayer.Play();
        }


      }



    }
}
