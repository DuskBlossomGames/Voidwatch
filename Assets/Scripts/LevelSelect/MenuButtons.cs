using System.Collections;
using System.Collections.Generic;
using Static_Info;
using UnityEngine;
using UnityEngine.SceneManagement;

using static Static_Info.PlayerData;
using static Static_Info.MerchantData;
using static Static_Info.LevelSelectData;
public class MenuButtons : MonoBehaviour
{
    public void Restart()
    {
        Debug.Log("Restart");
        Destroy(StaticInfoHolder.instance.gameObject); // reset static info
        SceneManager.LoadScene("Boot"); // return to boot
    }

    public void Quit()
    {
        UnityEngine.Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
