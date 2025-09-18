using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;
using System.IO;

namespace SplashScreen
{
    public class SplashListener : MonoBehaviour
    {
        public string streamingAssetsPath;
        public float clipLength;
        
        private void Start()
        {
            GetComponent<VideoPlayer>().url = Path.Combine(Application.streamingAssetsPath, streamingAssetsPath);
            GetComponent<VideoPlayer>().loopPointReached += _ => SceneManager.LoadScene("TitleScreen");

            StartCoroutine(Bypass());

        }

        private IEnumerator Bypass(){
          yield return new WaitForSeconds(clipLength);
          SceneManager.LoadScene("TitleScreen");
        }
    }
}
