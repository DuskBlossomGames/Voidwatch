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
        public interface IServiceGood {void Invoke(ShopSceneManager caller);}

        public string name;
        public Image icon;
        public List<Good?> goods;
    }

    public uint shopID; //Supplied by Scene spawner
    public MerchantData data;
    public Scriptable_Objects.PlayerData playerData;
    public GameObject selectIcon;
    MerchantData.MerchantObj _shop;
    public int cPos = 0;
    public int mPos = 0;

    private void Start()
    {
        selectIcon = transform.GetChild(0).GetChild(1).gameObject;
        MerchantData.MerchantObj shop = new MerchantData.MerchantObj();
        bool succ = data.shops.TryGetValue(data.currentShopID, out shop);
        if (!succ)
        {
            shop = CreateNewShop();
            Debug.Log("Created New Shop");
            data.shops.Add(data.currentShopID, shop);
        }

        shopID = data.currentShopID;

        _shop = shop;
        CreateScene();
    }

    public void Update()
    {
        var currMerchant = _shop.merchants[(int)_shop.merchantID];
        transform.GetChild(2).GetChild(2).GetComponent<Text>().text = playerData.Scrap.ToString();
        mPos = currMerchant.goods.Count - 1;
        if (Input.GetKeyDown("s"))
        {
            cPos += 1;
        } else if (Input.GetKeyDown("w"))
        {
            cPos -= 1;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            var item = currMerchant.goods[cPos];
            if (item.HasValue && item?.currentPrice <= playerData.Scrap)
            {
                playerData.Scrap -= (float) item?.currentPrice;
                switch (item?.uos)
                {
                    case Merchant.Good.GoodType.Service:
                        item?.serviceFunc.Invoke(this);
                        break;
                    case Merchant.Good.GoodType.Upgrade:
                        playerData.upgrades.Add(new UpgradeInstance(item?.serviceUpgrade));
                        break;
                    case Merchant.Good.GoodType.Weapon:
                        playerData.weapons.Add(item?.serviceWeapon);
                        break;
                }
                currMerchant.goods[cPos] = null;
                UpdateScene();
            }
        }

        cPos = (cPos = (cPos > mPos) ? mPos : cPos) < 0 ? 0 : cPos;


        var rt = selectIcon.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(rt.anchorMin.x, .8f - .2f * cPos);
        rt.anchorMax = new Vector2(rt.anchorMax.x, 1f - .2f * cPos);
    }

    void CreateScene()
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

            var shs = transform.GetChild(0).GetChild(i+2).GetComponent<ShopUpgradeSlot>();
            shs.oldPrice = (float) good?.originalPrice;
            shs.currPrice = (float)good?.currentPrice;
            shs.objName = good?.name;
            shs.InvokeUpdate();
        }
        Destroy(sampleSlot);

    }
    void UpdateScene()
    {
        var currMerchant = _shop.merchants[(int)_shop.merchantID];
        for (int i = 0; i < currMerchant.goods.Count; i++)
        {
            var good = currMerchant.goods[i];
            var curr = transform.GetChild(0).GetChild(i + 1);
            var shs = curr.GetComponent<ShopUpgradeSlot>();
            if (good.HasValue)
            {
                shs.isNull = false;
                shs.oldPrice = (float)good?.originalPrice;
                shs.currPrice = (float)good?.currentPrice;
                shs.objName = good?.name;
            } else
            {
                shs.isNull = true;
            }
            shs.InvokeUpdate();
        }
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
                serviceFunc = new Repairer(),
            });
        ret.goods.Add(
            new Merchant.Good
            {
                name = "Repair",
                desc = "Repair your ship restoring full health 2",
                currentPrice = 100,
                originalPrice = 100,
                uos = Merchant.Good.GoodType.Service,
                serviceFunc = new Repairer(),
            });


        return ret;
    }
    
    public class Repairer : Merchant.IServiceGood
    {
        public void Invoke(ShopSceneManager caller)
        {
            caller.playerData.Health = caller.playerData.playerMaxHealth;
        }
    }

    public void ReturnScene()
    {
        //Debug.Log("Jump");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Upgrades2");
    }
}


