using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Singletons.Static_Info;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using static Singletons.Static_Info.PlayerData;
using static Singletons.Static_Info.GunInfo;
using Random = UnityEngine.Random;


namespace Shop
{
    public class ShopManager : MonoBehaviour
    {
        public TextMeshProUGUI scrapDisplay, healCost;
        public TextMeshProUGUI[] boostTitles;
        public Image[] boostIcons;
        public TextMeshProUGUI[] boostCosts;
        
        public Button healButton;
        public Button[] boostButtons;
        
        public RectTransform healthBar, healthPreview;
        public ChainController[] boostChains;

        public float scrapPerThouHp;
        
        private int _healCost;
        private readonly int[] _boostCosts = new int[3];
        private List<BoostableStat> _stats;

        private readonly Timer _healPreviewTimer = new();
        private int _previewDir = -1;
        
        private void Start()
        {
            _stats = BoostableStat.STATS.OrderBy(_=>Random.value).Take(3).ToList();
            _healPreviewTimer.Value = 0.085f;
            _healPreviewTimer.SetValue(0);

            for (var i = 0; i < 3; i++)
            {
                boostTitles[i].text = _stats[i].name + " Boost";
                boostIcons[i].sprite = PlayerDataInstance.BoostableStatSprites[_stats[i].name];
            }
            
            UpdateShop();
        }

        private static float HealedHealth => Mathf.Min(PlayerDataInstance.maxHealth, 
            PlayerDataInstance.Health + 0.25f * PlayerDataInstance.maxHealth);
        public void Heal()
        {
            if (PlayerDataInstance.Scrap < _healCost) return;

            PlayerDataInstance.Scrap -= _healCost;
            PlayerDataInstance.Health = HealedHealth;

            // _healPreviewTimer.SetValue(0);
            UpdateShop();
        }

        public void HealPreview(bool show)
        {
            _previewDir = show ? 1 : -1;
        }
        
        public void Boost(int idx)
        {
            if (PlayerDataInstance.Scrap < _boostCosts[idx]) return;

            PlayerDataInstance.Scrap -= _boostCosts[idx];
            _stats[idx].Boosts++;
            
            UpdateShop();
        }

        private void SetPrices()
        {
            _healCost = Mathf.RoundToInt(0.25f * PlayerDataInstance.maxHealth / 1000f * scrapPerThouHp * 5) / 5;

            for (var i = 0; i < 3; i++)
            {
                _boostCosts[i] = BoostableStat.BOOST_COSTS[_stats[i].Boosts];
            }
        }

        private void UpdateShop()
        {
            SetPrices();
            
            scrapDisplay.text = PlayerDataInstance.Scrap.ToString();
            
            healCost.text = _healCost.ToString();
            healButton.interactable = PlayerDataInstance.Health < PlayerDataInstance.maxHealth && PlayerDataInstance.Scrap >= _healCost;

            for (var i = 0; i < 3; i++)
            {
                boostCosts[i].text = _boostCosts[i].ToString();
                boostButtons[i].interactable = _stats[i].Boosts < 3 && PlayerDataInstance.Scrap >= _boostCosts[i];
                boostChains[i].Unlocked = _stats[i].Boosts;
            }

            healthBar.anchorMin = new Vector2(PlayerDataInstance.Health / PlayerDataInstance.maxHealth, healthBar.anchorMin.y);
            healthPreview.anchorMin = new Vector2(PlayerDataInstance.Health / PlayerDataInstance.maxHealth, healthPreview.anchorMin.y);
        }

        private void Update()
        {
            _healPreviewTimer.Update(_previewDir);
            healthPreview.anchorMax =
                new Vector2(
                    Mathf.Lerp(PlayerDataInstance.Health, HealedHealth, _healPreviewTimer.Progress) /
                    PlayerDataInstance.maxHealth, healthPreview.anchorMax.y);
        }

        public void Exit()
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
