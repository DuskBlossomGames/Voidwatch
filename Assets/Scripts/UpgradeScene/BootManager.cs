using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    public MerchantData merchantData;
    public Scriptable_Objects.PlayerData playerData;
    void Start()
    {
        playerData.Scrap = 0;
        merchantData.currentShopID = 0;
        merchantData.shops = new Dictionary<uint, MerchantData.MerchantObj>();
        SceneManager.LoadScene("Shop");
    }
}
