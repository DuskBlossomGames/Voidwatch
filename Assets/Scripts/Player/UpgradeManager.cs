using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Menus;
using Singletons;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Util;
using Upgrade = Player.UpgradePlayer.Upgrade;
using static Singletons.Static_Info.PlayerData;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace Player
{
    public class UpgradeManager : MonoBehaviour
    {
        public Button reroll, pilfer;
    
        public GameObject minimap;
        public RectTransform titleBox, title, subtitle;
        public RectTransform[] upgrades;
        public CanvasGroup everythingElse;
        public float minimapFadeTime, titleTime, titleWaitTime, subtitleFadeTime, subtitleWaitTime, slideTime, slideWaitTime, fadeInTime;
        public float startAnchorMin, startAnchorMax;
        public int debugUpgrade = -1; // TODO: remove

        public AudioClip upgradeSound;

        public Movement playMov;
        public FollowPlayer followPlayer;

        private Upgrade[] _upgrades;

        public GraphicRaycaster raycaster;
        
        private void Update()
        {
#if UNITY_EDITOR
            if (debugUpgrade != -1 && InputManager.GetKeyDown(KeyCode.LeftBracket)) UpgradePlayer.UPGRADES[debugUpgrade].Apply(); 
#endif
        }

        private void SetUpgrades()
        {
            _upgrades = UpgradePlayer.GetRandomUpgrades(3);
            if (debugUpgrade != -1) _upgrades[0] = UpgradePlayer.UPGRADES[debugUpgrade];
        }

        public void Show()
        {
            SetUpgrades();
            StartCoroutine(DoShow());
        }
    
        private IEnumerator DoShow()
        {
            Debug.Log("SHOWING");
            playMov.SetInputBlocked(true);
            playMov.autoPilot = true;
            playMov.GetComponent<PlayerDamageable>().godmode = true;
            followPlayer.suppress = true;
            
            var rawImage = minimap.GetComponentInChildren<RawImage>();
            var border = minimap.GetComponentsInChildren<Image>()[1];
            var startAlpha = rawImage.color.a;
            for (float t = 0; t < minimapFadeTime; t += Time.fixedDeltaTime)
            {
                rawImage.SetAlpha(startAlpha*(1 - t / minimapFadeTime));
                border.SetAlpha(1 - t / minimapFadeTime);
                yield return new WaitForFixedUpdate();
            }
        
            var endAnchorMin = titleBox.anchorMin.y;
            var endAnchorMax = titleBox.anchorMax.y;
        
            titleBox.anchorMin = new Vector2(titleBox.anchorMin.x, startAnchorMin);
            titleBox.anchorMax = new Vector2(titleBox.anchorMax.x, startAnchorMax);

            var textbox = title.GetComponent<TextMeshProUGUI>();
            var text = textbox.text;
            textbox.text = "";
            title.gameObject.SetActive(true);
            foreach (var c in text)
            {
                textbox.text += c;
                yield return new WaitForSeconds(titleTime / text.Length);
            }

            yield return new WaitForSeconds(titleWaitTime);
        
            var subtitleBox = subtitle.GetComponent<TextMeshProUGUI>();
            subtitleBox.SetAlpha(0);
            subtitle.gameObject.SetActive(true);
            for (float t = 0; t < subtitleFadeTime; t += Time.fixedDeltaTime)
            {
                subtitleBox.SetAlpha(t / subtitleFadeTime);
                yield return new WaitForFixedUpdate();
            }
        
            yield return new WaitForSeconds(subtitleWaitTime);

            for (float t = 0; t < slideTime; t += Time.fixedDeltaTime)
            {
                titleBox.anchorMin = new Vector2(titleBox.anchorMin.x, Mathf.SmoothStep(startAnchorMin, endAnchorMin, t / slideTime));
                titleBox.anchorMax = new Vector2(titleBox.anchorMax.x, Mathf.SmoothStep(startAnchorMax, endAnchorMax, t / slideTime));
                yield return new WaitForFixedUpdate();
            }
        
            yield return new WaitForSeconds(slideWaitTime);

            everythingElse.alpha = 0;
            UpdateUpgrades();
            everythingElse.gameObject.SetActive(true);
            for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
            {
                everythingElse.alpha = t/fadeInTime;
                yield return new WaitForFixedUpdate();
            }
        }

        private void UpdateUpgrades()
        {
            for (var i = 0; i < 3; i++)
            {
                upgrades[i].GetChild(0).GetChild(1).GetComponent<Image>().sprite = PlayerDataInstance.UpgradeSprites[_upgrades[i].Title];
                upgrades[i].GetChild(0).GetChild(2).GetComponent<Image>().sprite = PlayerDataInstance.RaritySprites[_upgrades[i].Rarity.Name][0];
                upgrades[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Title;
                upgrades[i].GetChild(2).GetComponent<Image>().sprite = PlayerDataInstance.RaritySprites[_upgrades[i].Rarity.Name][1];
                upgrades[i].GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Description;
                upgrades[i].GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Quip;
            }

            reroll.interactable = PlayerDataInstance.Scrap >= _rerollCost;
        }

        public void SelectUpgrade(int i)
        {
            _upgrades[i].Apply();
            DontDestroyOnLoad(AudioPlayer.Play(upgradeSound, null, 1f, 1.1f));
            StartCoroutine(ExitAfter(0.3f));

            // disable other options
            for (var j = 0; j < 3; j++)
            {
                if (j == i)
                {
                    foreach (var b in upgrades[j].GetComponentsInChildren<Button>())
                    {
                        var block = b.colors;
                        block.disabledColor = b.colors.normalColor;
                        b.colors = block;
                        b.interactable = false;
                    }
                    continue;
                }
                
                foreach (var b in upgrades[j].GetComponentsInChildren<Button>()) b.interactable = false;
                upgrades[j].GetChild(1).GetComponent<TextMeshProUGUI>().SetAlpha(0.5f);
            }
            reroll.interactable = pilfer.interactable = false;
        }

        private IEnumerator ExitAfter(float time)
        {
            yield return new WaitForSeconds(time);
            Exit();
        }

        private int _rerollCost = 50;
        private int _numRerolls;
        public void Reroll()
        {
            if (PlayerDataInstance.Scrap < _rerollCost) return;
            
            PlayerDataInstance.Scrap -= _rerollCost;
            _rerollCost = Mathf.RoundToInt(_rerollCost * Mathf.Max(1.8f - 0.1f * ++_numRerolls, 1.2f) / 10) * 10;
            reroll.GetComponentInChildren<TextMeshProUGUI>().text = _rerollCost.ToString();
            
            SetUpgrades();
            UpdateUpgrades();
        }

        public void Scavenge()
        {
            var scrap = Random.Range(200, 300);
            PlayerDataInstance.Scrap += scrap;

            raycaster.enabled = false; // just disable all interaction at this point
            
            var sdc = FindObjectsByType<ScrapDisplayController>(FindObjectsSortMode.None)[0];
            sdc.FinishTransfer += Exit;
            
            for (var i = 0; i < 3; i++)
            {
                foreach (var b in upgrades[i].GetComponentsInChildren<Button>()) b.interactable = false;
                upgrades[i].GetChild(1).GetComponent<TextMeshProUGUI>().SetAlpha(0.5f);
            }
            reroll.interactable = false;
        }

        private void Exit()
        {
            SceneManager.LoadScene("LevelSelect");
        }
    }
}
