using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

namespace SplashScreen
{
    public class SplashListener : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<VideoPlayer>().loopPointReached += _ => SceneManager.LoadScene("TitleScreen");

            StartCoroutine(Bypass());

        }

        private IEnumerator Bypass(){
          yield return new WaitForSeconds((float) GetComponent<VideoPlayer>().clip.length);
          SceneManager.LoadScene("TitleScreen");
        }
    }
}
