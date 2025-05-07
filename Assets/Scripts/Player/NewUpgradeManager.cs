using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Upgrade = UpgradePlayer.Upgrade;
using Util;

public class NewUpgradeManager : MonoBehaviour
{
    public AssetLabelReference borderSprites, upgradeSprites;
    private readonly Dictionary<Upgrade, Sprite> _upgradeSprites = new();
    private readonly Dictionary<UpgradePlayer.Rarity, Sprite[]> _raritySprites = new();

    public RawImage minimap;
    public RectTransform titleBox, title, subtitle;
    public RectTransform[] upgrades;
    public CanvasGroup everythingElse;
    public float minimapFadeTime, titleTime, titleWaitTime, subtitleFadeTime, subtitleWaitTime, slideTime, slideWaitTime, fadeInTime;
    public float startAnchorMin, startAnchorMax;
    
    public Player.Movement playMov;
    public Player.FollowPlayer followPlayer;
    
    private readonly Upgrade[] _upgrades = new Upgrade[3];

    private void Start()
    {
        Addressables.LoadAssetsAsync<Sprite>(borderSprites, null).Completed += handle =>
        {
            var byName = handle.Result.ToDictionary(s => s.name, s => s);
            foreach (var rarity in UpgradePlayer.Rarity.ALL)
            {
                _raritySprites[rarity] = new[] { byName[rarity.Sprite], byName[rarity.Sprite + "-BOX"] };
            }
        };
        Addressables.LoadAssetsAsync<Sprite>(upgradeSprites, null).Completed += handle =>
        {
            var byName = handle.Result.ToDictionary(s => s.name, s => s);
            foreach (var upgrade in UpgradePlayer.UPGRADES)
            {
                _upgradeSprites[upgrade] = byName[upgrade.Title];
            }
        };
    }

    public void Show()
    {
        for (var i = 0; i < 3; i++) _upgrades[i] = UpgradePlayer.GetRandomUpgrade();
        StartCoroutine(DoShow());
    }
    
    private IEnumerator DoShow()
    {
        playMov.SetInputBlocked(true);
        playMov.autoPilot = true;
        playMov.gameObject.GetComponent<Spawnables.Player.PlayerDamageable>().godmode = true;
        followPlayer.suppres = true;

        var startAlpha = minimap.color.a;
        for (float t = 0; t < minimapFadeTime; t += Time.fixedDeltaTime)
        {
            minimap.SetAlpha(startAlpha*(1 - t / minimapFadeTime));
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
            upgrades[i].GetChild(0).GetComponent<Image>().sprite = _upgradeSprites[_upgrades[i]];
            upgrades[i].GetChild(1).GetComponent<Image>().sprite = _raritySprites[_upgrades[i].Rarity][0];
            upgrades[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Title;
            upgrades[i].GetChild(3).GetComponent<Image>().sprite = _raritySprites[_upgrades[i].Rarity][1];
            upgrades[i].GetChild(3).GetChild(0).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Description;
            upgrades[i].GetChild(3).GetChild(1).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Quip;
        }
    }

    public void SelectUpgrade(int i)
    {
        _upgrades[i].Apply();
        SceneManager.LoadScene("LevelSelect");
    }

    public void Reroll()
    {
        for (var i = 0; i < 3; i++) _upgrades[i] = UpgradePlayer.GetRandomUpgrade();
        UpdateUpgrades();
    }

    public void Scavenge()
    {
        
    }
}
