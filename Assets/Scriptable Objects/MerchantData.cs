using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DMG", menuName = "ScriptableObjects/MerchantData", order = 1)]
public class MerchantData : ScriptableObject
{
    public SerializedDict<uint, MerchantObj> Shops;
    public uint currentShopID;
    public struct MerchantObj
    {
        public List<ShopSceneManager.Merchant> merchants;
        public uint merchantID;
    }
}
