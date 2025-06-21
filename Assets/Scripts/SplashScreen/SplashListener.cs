using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace SplashScreen
{
    public class SplashListener : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<VideoPlayer>().loopPointReached += _ => SceneManager.LoadScene("TitleScreen");
        }
    }
}