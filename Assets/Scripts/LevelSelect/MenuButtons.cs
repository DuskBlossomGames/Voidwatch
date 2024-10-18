using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public MerchantData merchantData;
    public Scriptable_Objects.PlayerData playerData;
    public void Restart()
    {
        Debug.Log("Restart");
        playerData.Scrap = 0;
        merchantData.currentShopID = 0;
        merchantData.shops = new SerializedDict<uint, MerchantData.MerchantObj>();
        SceneManager.LoadScene("LevelSelect");
    }

    public void Quit()
    {
        UnityEngine.Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }
}
