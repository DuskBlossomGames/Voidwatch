using System.Collections;
using System.Collections.Generic;
using ShopButBetter;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Static_Info.PlayerData;
using static Static_Info.GunInfo;
using UnityEngine.SceneManagement;


public class ShopManagerAgain : MonoBehaviour
{
    public RectTransform lhp;
    public RectTransform mhp;
    public RectTransform rhp;
    public RectTransform hpBar;
    public TMP_Text lbut;
    public TMP_Text mbut;
    public TMP_Text rbut;
    public TMP_Text wbo;
    public TMP_Text hbo;
    public TMP_Text sbo;
    public TMP_Text scrapDisplay;
    public bool lf = true;
    public bool mf = true;
    public bool rf = true;

    public ChainController dmgChain, hpChain, speedChain;

    public float boostCost;
    public float scrapPerThouHp;
    public float cheapDiscount;
    public float midDiscount;
    public float honestFactor;

    public List<float> boostPercentages = new();
    private string[] _roman = new string[] { "I", "II", "III", "IV", "V" };

    private int[] _repairs = new int[3];


    public int RoundPrice(float price, int minPrice = 0)
    {
        float factor = 40; //Magic number to produce desired rounding, here we want factors of 8 and 10.
        float mag = Mathf.Pow(10f, Mathf.Log10(price));
        float pdig = Mathf.Floor(price / mag); //Leading digit
        float rem = (price / mag) % 1;
        float mrem = Mathf.Round(rem * factor) / factor;
        float tprice = mag * (pdig + mrem);
        int rprice = 5 * Mathf.CeilToInt(tprice / 5); //Round to nearest 5
        if (rprice < minPrice) rprice = minPrice + 5;
        return rprice;
    }

    public void SetPrices()
    {
        var PDI = PlayerDataInstance;
        float perDmg = 1 - PDI.Health / PDI.maxHealth;
        var MHP = PDI.maxHealth;
        float mercy = Mathf.Pow(perDmg, 1f / 3f);
        float mul = Mathf.Lerp(honestFactor, 1f, mercy);
        
        int min;
        _repairs[0] = min = RoundPrice(mul * MHP * scrapPerThouHp / 1000f * cheapDiscount); // 1/3 Cost
        _repairs[1] = min = RoundPrice(mul * MHP * scrapPerThouHp / 1000f * midDiscount, min); // 2/3 Cost
        _repairs[2] = min = RoundPrice(mul * MHP * scrapPerThouHp / 1000f, min); // Full Cost

        lbut.text = $"{_repairs[0]:# ### ###}";
        mbut.text = $"{_repairs[1]:# ### ###}";
        rbut.text = $"{_repairs[2]:# ### ###}";

        // TODO: show cost (and maybe boost) somewhere...
        // if (PDI.healthBoosts < 5) {
        //     var rbc = boostCost * (PDI.healthBoosts + 1) * (PDI.healthBoosts + 1);
        //     hbo.text = $"Lvl {_roman[PDI.healthBoosts]}: +{boostPercentages[PDI.healthBoosts]}%\n{rbc} SCRAP";
        // } else {
        //     hbo.text = $"Max Lvl";
        // }
        //
        // if (PDI.damageBoosts < 5) {
        //     var rbc = boostCost * (PDI.damageBoosts + 1) * (PDI.damageBoosts + 1);
        //     wbo.text = $"Lvl {_roman[PDI.damageBoosts]}: +{boostPercentages[PDI.damageBoosts]}%\n{rbc} SCRAP";
        // } else {
        //     wbo.text = $"Max Lvl";
        // }
        //
        // if (PDI.speedBoosts < 5) {
        //     var rbc = boostCost * (PDI.speedBoosts + 1) * (PDI.speedBoosts + 1);
        //     sbo.text = $"Lvl {_roman[PDI.speedBoosts]}: +{boostPercentages[PDI.speedBoosts]}%\n{rbc} SCRAP";
        // } else {
        //     sbo.text = $"Max Lvl";
        // }
    }


    public void Update()
    {
        Draw();
    }

    public void LowerHeal()
    {
        var PDI = PlayerDataInstance;
        if (!lf || PDI.Scrap < _repairs[0]) return;
        lf = !lf;
        PDI.Health = (1 - 2 * (1 - PDI.Health / PDI.maxHealth) / 3) * PDI.maxHealth;
        PDI.Scrap -= _repairs[0];
    }

    public void MiddleHeal()
    {
        var PDI = PlayerDataInstance;
        if (!mf || PDI.Scrap < _repairs[1]) return;
        mf = !mf;
        PDI.Health = (1 - 1 * (1 - PDI.Health / PDI.maxHealth) / 3) * PDI.maxHealth;
        PDI.Scrap -= _repairs[1];
    }

    public void HighHeal()
    {
        var PDI = PlayerDataInstance;
        if (!rf || PDI.Scrap < _repairs[2]) return;
        rf = !rf;
        PDI.Health = PDI.maxHealth;
        PDI.Scrap -= _repairs[2];
    }

    public void HBoost()
    {
        var PDI = PlayerDataInstance;
        var rbc = boostCost * (PDI.healthBoosts + 1) * (PDI.healthBoosts + 1);
        if (PDI.healthBoosts >= 5 || PDI.Scrap < rbc) return;
        float per = PDI.Health / PDI.maxHealth;
        PDI.maxHealth = Mathf.CeilToInt(PDI.maxHealth * (1f + boostPercentages[PDI.healthBoosts] / 100f));
        PDI.Health = PDI.maxHealth * per;
        PDI.healthBoosts += 1;
        PDI.Scrap -= rbc;
    }

    public void WBoost()
    {
        var PDI = PlayerDataInstance;
        var rbc = boostCost * (PDI.damageBoosts + 1) * (PDI.damageBoosts + 1);
        if (PDI.damageBoosts >= 5 || PDI.Scrap < rbc) return;
        GunInfoInstance.dmgMod *= 1f + boostPercentages[PDI.damageBoosts] / 100f;
        PDI.damageBoosts += 1;
        PDI.Scrap -= rbc;
    }

    public void SBoost()
    {
        var PDI = PlayerDataInstance;
        var rbc = boostCost * (PDI.speedBoosts + 1) * (PDI.speedBoosts + 1);
        if (PDI.speedBoosts >= 5 || PDI.Scrap < rbc) return;
        PlayerDataInstance.speedLimit *= 1f + boostPercentages[PDI.speedBoosts] / 100f;
        PDI.speedBoosts += 1;
        PDI.Scrap -= rbc;
    }

    public void Draw()
    {
        var PDI = PlayerDataInstance;
        var prog = PDI.Health / PDI.maxHealth;
        hpBar.anchorMin = new Vector2(prog, hpBar.anchorMin.y);
        
        // magic constants visually derived
        lhp.anchorMin = new Vector2(.09f + .77f * (1 - 2 * (1 - prog) / 3), lhp.anchorMin.y);
        lhp.anchorMax = new Vector2(.15f + .77f * (1 - 2 * (1 - prog) / 3), lhp.anchorMax.y);
        
        mhp.anchorMin = new Vector2(.09f + .77f * (1 - 1 * (1 - prog) / 3), mhp.anchorMin.y);
        mhp.anchorMax = new Vector2(.15f + .77f * (1 - 1 * (1 - prog) / 3), mhp.anchorMax.y);
        
        rhp.anchorMin = new Vector2(.09f + .77f * (1f), rhp.anchorMin.y);
        rhp.anchorMax = new Vector2(.15f + .77f * (1f), rhp.anchorMax.y);
        
        scrapDisplay.text = $"{PDI.Scrap:### ### ### ##0}";

        dmgChain.Unlocked = PDI.damageBoosts;
        hpChain.Unlocked = PDI.healthBoosts;
        speedChain.Unlocked = PDI.speedBoosts;
        
        SetPrices();
    }

    public void Exit()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
