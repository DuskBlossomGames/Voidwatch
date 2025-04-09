using System.Collections.Generic;
using UnityEngine;
using Util;

namespace Static_Info
{
    public class MerchantData : MonoBehaviour
    {
        public static MerchantData MerchantDataInstance => StaticInfoHolder.instance.GetCachedComponent<MerchantData>();
        
        public SerializedDict<uint, MerchantObj> Shops;
        public uint currentShopID;
        public struct MerchantObj
        {
            public List<ShopSceneManager.Merchant> merchants;
            public uint merchantID;
        }
    }
}