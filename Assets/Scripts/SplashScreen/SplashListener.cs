using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;

namespace SplashScreen
{
    public class SplashListener : MonoBehaviour
    {
      private IEnumerator splashcoroutine;

        private void Start()
        {
            GetComponent<VideoPlayer>().loopPointReached += _ => SceneManager.LoadScene("TitleScreen");

            splashcoroutine = SplashScreenBypass(8.0f);
            StartCoroutine(splashcoroutine);

        }

        private IEnumerator SplashScreenBypass(float bypassTime){
          yield return new WaitForSeconds(bypassTime);
          SceneManager.LoadScene("TitleScreen");
        }
    }
}
