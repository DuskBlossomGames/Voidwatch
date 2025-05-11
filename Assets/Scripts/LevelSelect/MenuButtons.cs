using Static_Info;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void Restart()
    {
        Debug.Log("Restart");
        Destroy(StaticInfoHolder.Instance.gameObject); // reset static info
        SceneManager.LoadScene("TitleScreen"); // return to boot
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}
