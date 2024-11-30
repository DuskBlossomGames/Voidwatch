using System.Collections;
using System.Collections.Generic;
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
    public RectTransform hpbBar;
    public RectTransform dmgbBar;
    public RectTransform spdbBar;
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

        lbut.text = $"33% Repair\n{_repairs[0]:# ### ###} SCRAP";
        mbut.text = $"67% Repair\n{_repairs[1]:# ### ###} SCRAP";
        rbut.text = $"Full Repair\n{_repairs[2]:# ### ###} SCRAP";

        hbo.text = $"Lvl {_roman[PDI.healthBoosts]}: +{boostPercentages[PDI.healthBoosts]}%\n{boostCost} SCRAP";
        wbo.text = $"Lvl {_roman[PDI.damageBoosts]}: +{boostPercentages[PDI.damageBoosts]}%\n{boostCost} SCRAP";
        sbo.text = $"Lvl {_roman[PDI.speedBoosts]}: +{boostPercentages[PDI.speedBoosts]}%\n{boostCost} SCRAP";
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
        if (PDI.healthBoosts >= 5 || PDI.Scrap < boostCost) return;
        float per = PDI.Health / PDI.maxHealth;
        PDI.maxHealth *= Mathf.CeilToInt(1f + boostPercentages[PDI.healthBoosts] / 100f);
        PDI.Health = PDI.maxHealth * per;
        PDI.healthBoosts += 1;
        PDI.Scrap -= boostCost;
    }

    public void WBoost()
    {
        var PDI = PlayerDataInstance;
        if (PDI.damageBoosts >= 5 || PDI.Scrap < boostCost) return;
        GunInfoInstance.dmgMod *= Mathf.CeilToInt(1f + boostPercentages[PDI.damageBoosts] / 100f);
        PDI.damageBoosts += 1;
        PDI.Scrap -= boostCost;
    }

    public void SBoost()
    {
        Debug.LogWarning("Unimplemented");
    }

    public void Draw()
    {
        var PDI = PlayerDataInstance;
        var prog = PDI.Health / PDI.maxHealth;
        hpBar.anchorMax = new Vector2(prog, hpBar.anchorMax.y);
        lhp.anchorMin = new Vector2(.02f + .96f * (1- 2*(1-prog)/3), lhp.anchorMax.y);
        lhp.anchorMax = new Vector2(.02f + .96f * (1 - 2* (1 - prog) / 3), lhp.anchorMax.y);
        mhp.anchorMin = new Vector2(.02f + .96f * (1 - 1 * (1 - prog) / 3), lhp.anchorMax.y);
        mhp.anchorMax = new Vector2(.02f + .96f * (1 - 1 * (1 - prog) / 3), lhp.anchorMax.y);
        rhp.anchorMin = new Vector2(.02f + .96f * (1f), lhp.anchorMax.y);
        rhp.anchorMax = new Vector2(.02f + .96f * (1f), lhp.anchorMax.y);

        hpbBar.anchorMax = new Vector2(PDI.healthBoosts / 5f, hpBar.anchorMax.y);
        dmgbBar.anchorMax = new Vector2(PDI.damageBoosts / 5f, hpBar.anchorMax.y);
        spdbBar.anchorMax = new Vector2(PDI.speedBoosts / 5f, hpBar.anchorMax.y);

        scrapDisplay.text = $"{PDI.Scrap:### ### ### ##0} SCRAP";

        SetPrices();
    }

    public void Exit()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
