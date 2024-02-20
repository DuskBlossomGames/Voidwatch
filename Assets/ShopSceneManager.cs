using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Scriptable_Objects.Upgrades;

public class ShopSceneManager : MonoBehaviour
{
    public class Merchant
    {
        public struct Good
        {
            public enum GoodType
            {
                Upgrade,
                Service,
                Weapon,
            }

            public string name;
            public string desc;
            public int originalPrice;
            public int currentPrice;
            public GoodType uos;
            public IServiceGood? serviceFunc;
            public BaseUpgrade? serviceUpgrade;
            public BaseWeapon? serviceWeapon;
        }
        public interface IServiceGood {void Invoke();}

        public string name;
        public Image icon;
        public List<Good?> goods;
    }

    public uint shopID; //Supplied by Scene spawner
    public MerchantData data;
    MerchantData.MerchantObj _shop;

    private void Start()
    {
        MerchantData.MerchantObj shop = new MerchantData.MerchantObj();
        bool succ = data.shops.TryGetValue(data.currentShopID, out shop);
        if (!succ)
        {
            shop = CreateNewShop();
            Debug.Log("Created New Shop");
            data.shops.Add(data.currentShopID, shop);
        }

        _shop = shop;
        UpdateScene();
    }

    void UpdateScene()
    {
        var currMerchant = _shop.merchants[(int)_shop.merchantID];
        transform.GetChild(1).GetChild(1).GetComponent<Text>().text = currMerchant.name;

        var sampleSlot = transform.GetChild(0).GetChild(0).gameObject;
        for (int i = 0; i < currMerchant.goods.Count; i++)
        {
            var good = currMerchant.goods[i];
            var curr = Instantiate<GameObject>(sampleSlot,sampleSlot.transform.parent);
            var rt = curr.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(rt.anchorMin.x, .8f - .2f * i);
            rt.anchorMax = new Vector2(rt.anchorMax.x, 1f - .2f * i);

            if(good?.originalPrice != good?.currentPrice)
            {
                curr.transform.GetChild(0).GetChild(2).gameObject.En
            }
        }
        Destroy(sampleSlot);
    }


    MerchantData.MerchantObj CreateNewShop()
    {
        MerchantData.MerchantObj ret = new MerchantData.MerchantObj();
        ret.merchants = new List<Merchant>();
        for (int i = 0; i < 5; i++)
        {
            ret.merchants.Add(CreateNewMerchant());
        }
        
        ret.merchantID = 0;

        return ret;
    }
    Merchant CreateNewMerchant()
    {
        Merchant ret = new Merchant();
        ret.name = "Test Merchant";
        ret.goods = new List<Merchant.Good?>();
        ret.goods.Add(
            new Merchant.Good
            {
                name = "Repair",
                desc = "Repair your ship restoring full health",
                currentPrice = 50,
                originalPrice = 100,
                uos = Merchant.Good.GoodType.Service,
                serviceFunc = null,
            });
        

        return ret;
    }

}
