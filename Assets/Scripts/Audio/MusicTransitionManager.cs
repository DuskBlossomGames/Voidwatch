using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public AudioClip bossPhase2;
    private bool _playingIntro;

    public Scene levelSelect;
    public Scene levelPlay;
    public Scene levelBoss;

    private bool boss2Active;
    public float timeInterpolate;

    // Start is called before the first frame update
    void Awake(){
      if(instance==null){
        instance = this;
        DontDestroyOnLoad(instance);
      } else {
          Destroy(gameObject);

      }

    }

    void Update(){
      if(SceneManager.GetActiveScene() == levelPlay){

          if(audioPlayer.clip != battleIntro || audioPlayer.clip != battleLoop){

            if(audioPlayer.volume > 0f){

              audioPlayer.volume -= (1/timeInterpolate) * Time.deltaTime;
              Mathf.Clamp(audioPlayer.volume,0f,1f);

            } else{

              audioPlayer.clip = battleIntro;
              audioPlayer.Play();
              _playingIntro = true;

            }

          }

          if(audioPlayer.volume < 1f){

            audioPlayer.volume += (1/timeInterpolate) * Time.deltaTime;
            Mathf.Clamp(audioPlayer.volume,0f,1f);

          }

          if(!audioPlayer.isPlaying && _playingIntro){
            _playingIntro = false;
            audioPlayer.clip = battleLoop;
            audioPlayer.loop = true;
          }


      } else if(SceneManager.GetActiveScene() == levelBoss){

          if(audioPlayer.clip != bossIntro || audioPlayer.clip != bossLoop){

            if(audioPlayer.volume > 0f){

              audioPlayer.volume -= (1/timeInterpolate) * Time.deltaTime;
              Mathf.Clamp(audioPlayer.volume,0f,1f);

            } else{

              audioPlayer.clip = bossIntro;
              audioPlayer.Play();
              _playingIntro = true;

            }

          }

          if(audioPlayer.volume < 1f){

            audioPlayer.volume += (1/timeInterpolate) * Time.deltaTime;
            Mathf.Clamp(audioPlayer.volume,0f,1f);

          }

          if(!audioPlayer.isPlaying && _playingIntro){
            _playingIntro = false;
            audioPlayer.clip = bossLoop;
            audioPlayer.loop = true;
          }


      }

    /*  else{


      }*/



    }
}
