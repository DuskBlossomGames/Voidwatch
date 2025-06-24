using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Static_Info.PlayerData;
using static Static_Info.GunInfo;


namespace Shop
{
    public class ShopManager : MonoBehaviour
    {
        public RectTransform lhp;
        public RectTransform mhp;
        public RectTransform rhp;
        public RectTransform hpBar;
        public TMP_Text lbut;
        public TMP_Text mbut;
        public TMP_Text rbut;
        public TextMeshProUGUI dCost, hCost, sCost;
        public Button dButton, hButton, sButton, lhButton, mhButton, hhButton; 
        public TMP_Text scrapDisplay;

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
            return Mathf.Max(rprice, minPrice);
        }

        private void SetPrices()
        {
            var PDI = PlayerDataInstance;
            float mercy = Mathf.Pow(1 - PDI.Health / PDI.maxHealth, 1f / 3f);
            float mul = Mathf.Lerp(honestFactor, 1f, mercy);
        
            int min;
            var dmg = PDI.maxHealth - PDI.Health;
            _repairs[0] = min = RoundPrice(mul * dmg * scrapPerThouHp / 1000f * cheapDiscount); // 1/3 Cost
            _repairs[1] = min = RoundPrice(mul * dmg * scrapPerThouHp / 1000f * midDiscount, min); // 2/3 Cost
            _repairs[2] = min = RoundPrice(mul * dmg * scrapPerThouHp / 1000f, min); // Full Cost

            lbut.text = $"{_repairs[0]:# ### ###}";
            mbut.text = $"{_repairs[1]:# ### ###}";
            rbut.text = $"{_repairs[2]:# ### ###}";
        }


        public void Update()
        {
            Draw();
        }

        public void LowerHeal()
        {
            var PDI = PlayerDataInstance;
            if (PDI.Scrap < _repairs[0]) return;
            PDI.Health = (1 - 2 * (1 - PDI.Health / PDI.maxHealth) / 3) * PDI.maxHealth;
            PDI.Scrap -= _repairs[0];
        }

        public void MiddleHeal()
        {
            var PDI = PlayerDataInstance;
            if (PDI.Scrap < _repairs[1]) return;
            PDI.Health = (1 - 1 * (1 - PDI.Health / PDI.maxHealth) / 3) * PDI.maxHealth;
            PDI.Scrap -= _repairs[1];
        }

        public void HighHeal()
        {
            var PDI = PlayerDataInstance;
            if (PDI.Scrap < _repairs[2]) return;
            PDI.Health = PDI.maxHealth;
            PDI.Scrap -= _repairs[2];
        }

        public void HBoost()
        {
            var PDI = PlayerDataInstance;
            var rbc = boostCost * (PDI.HealthBoosts + 1) * (PDI.HealthBoosts + 1);
            if (PDI.HealthBoosts >= 5 || PDI.Scrap < rbc) return;
            var dmg = PDI.maxHealth - PDI.Health;
            PDI.maxHealth = Mathf.CeilToInt(PDI.maxHealth * (1f + boostPercentages[PDI.HealthBoosts] / 100f));
            PDI.Health = PDI.maxHealth - dmg;
            PDI.HealthBoosts += 1;
            PDI.Scrap -= (int) rbc;
        }

        public void WBoost()
        {
            var PDI = PlayerDataInstance;
            var rbc = boostCost * (PDI.DamageBoosts + 1) * (PDI.DamageBoosts + 1);
            if (PDI.DamageBoosts >= 5 || PDI.Scrap < rbc) return;
            GunInfoInstance.dmgMod *= 1f + boostPercentages[PDI.DamageBoosts] / 100f;
            PDI.DamageBoosts += 1;
            PDI.Scrap -= (int) rbc;
        }

        public void SBoost()
        {
            var PDI = PlayerDataInstance;
            var rbc = boostCost * (PDI.SpeedBoosts + 1) * (PDI.SpeedBoosts + 1);
            if (PDI.SpeedBoosts >= 5 || PDI.Scrap < rbc) return;
            PlayerDataInstance.speedLimit *= 1f + boostPercentages[PDI.SpeedBoosts] / 100f;
            PDI.SpeedBoosts += 1;
            PDI.Scrap -= (int) rbc;
        }

        private float _anchorMinStart = 0.065f, _anchorMaxStart = 0.125f, _anchorWidth = 0.765f;
        public void Draw()
        {
            var PDI = PlayerDataInstance;
            var prog = PDI.Health / PDI.maxHealth;
            hpBar.anchorMin = new Vector2(prog, hpBar.anchorMin.y);
        
            // magic constants visually derived
            lhp.anchorMin = new Vector2(_anchorMinStart + _anchorWidth * (1 - 2 * (1 - prog) / 3), lhp.anchorMin.y);
            lhp.anchorMax = new Vector2(_anchorMaxStart + _anchorWidth * (1 - 2 * (1 - prog) / 3), lhp.anchorMax.y);
        
            mhp.anchorMin = new Vector2(_anchorMinStart + _anchorWidth * (1 - 1 * (1 - prog) / 3), mhp.anchorMin.y);
            mhp.anchorMax = new Vector2(_anchorMaxStart + _anchorWidth * (1 - 1 * (1 - prog) / 3), mhp.anchorMax.y);
        
            rhp.anchorMin = new Vector2(_anchorMinStart + _anchorWidth * (1f), rhp.anchorMin.y);
            rhp.anchorMax = new Vector2(_anchorMaxStart + _anchorWidth * (1f), rhp.anchorMax.y);
        
            scrapDisplay.text = $"{PDI.Scrap:### ### ### ##0}";
            dCost.text = PDI.DamageBoosts < 5 ? $"{boostCost * (PDI.DamageBoosts + 1) * (PDI.DamageBoosts + 1):### ### ### ##0}" : "MAX";
            hCost.text = PDI.HealthBoosts < 5 ? $"{boostCost * (PDI.HealthBoosts + 1) * (PDI.HealthBoosts + 1):### ### ### ##0}" : "MAX";
            sCost.text = PDI.SpeedBoosts < 5 ? $"{boostCost * (PDI.SpeedBoosts + 1) * (PDI.SpeedBoosts + 1):### ### ### ##0}" : "MAX";

            dmgChain.Unlocked = PDI.DamageBoosts;
            hpChain.Unlocked = PDI.HealthBoosts;
            speedChain.Unlocked = PDI.SpeedBoosts;

            lhButton.interactable = prog < 1 && PDI.Scrap >= _repairs[0];
            mhButton.interactable = prog < 1 && PDI.Scrap >= _repairs[1];
            hhButton.interactable = prog < 1 && PDI.Scrap >= _repairs[2];
        
            dButton.interactable = PDI.DamageBoosts < 5 && PDI.Scrap >= boostCost * (PDI.DamageBoosts + 1) * (PDI.DamageBoosts + 1);
            hButton.interactable = PDI.HealthBoosts < 5 && PDI.Scrap >= boostCost * (PDI.HealthBoosts + 1) * (PDI.HealthBoosts + 1);
            sButton.interactable = PDI.SpeedBoosts < 5 && PDI.Scrap >= boostCost * (PDI.SpeedBoosts + 1) * (PDI.SpeedBoosts + 1);
        
            SetPrices();
        }

        public void Exit()
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
