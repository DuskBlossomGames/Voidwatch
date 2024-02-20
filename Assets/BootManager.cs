using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{
    public MerchantData merchantData;
    void Start()
    {
        merchantData.currentShopID = 0;
        merchantData.shops = new Dictionary<uint, MerchantData.MerchantObj>();
        SceneManager.LoadScene("Shop");
    }
}
