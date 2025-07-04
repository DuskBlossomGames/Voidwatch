using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Player;
using Spawnables.Player;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using Upgrade = UpgradePlayer.Upgrade;

public class NewUpgradeManager : MonoBehaviour
{
    public AssetLabelReference borderSprites, upgradeSprites;
    private readonly Dictionary<string, Sprite> _upgradeSprites = new();
    private readonly Dictionary<string, Sprite[]> _raritySprites = new();

    public RawImage minimap;
    public RectTransform titleBox, title, subtitle;
    public RectTransform[] upgrades;
    public CanvasGroup everythingElse;
    public float minimapFadeTime, titleTime, titleWaitTime, subtitleFadeTime, subtitleWaitTime, slideTime, slideWaitTime, fadeInTime;
    public float startAnchorMin, startAnchorMax;
    public int debugUpgrade = -1; // TODO: remove
    
    public Movement playMov;
    public FollowPlayer followPlayer;

    private Upgrade[] _upgrades;

    private void Start()
    {
        Addressables.LoadAssetsAsync<Sprite>(borderSprites, null).Completed += handle =>
        {
            var byName = handle.Result.ToDictionary(s => s.name, s => s);
            foreach (var rarity in UpgradePlayer.Rarity.ALL)
            {
                _raritySprites[rarity.Name] = new[] { byName[rarity.Name], byName[rarity.Name + "-BOX"] };
            }
        };
        Addressables.LoadAssetsAsync<Sprite>(upgradeSprites, null).Completed += handle =>
        {
            var byName = handle.Result.ToDictionary(s => s.name, s => s);
            foreach (var upgrade in UpgradePlayer.UPGRADES)
            {
                _upgradeSprites[upgrade.Title] = byName[upgrade.Title];
            }
        };
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
        playMov.SetInputBlocked(true);
        playMov.autoPilot = true;
        playMov.GetComponent<PlayerDamageable>().godmode = true;
        followPlayer.suppress = true;

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
            upgrades[i].GetChild(0).GetComponent<Image>().sprite = _upgradeSprites[_upgrades[i].Title];
            upgrades[i].GetChild(1).GetComponent<Image>().sprite = _raritySprites[_upgrades[i].Rarity.Name][0];
            upgrades[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = _upgrades[i].Title;
            upgrades[i].GetChild(3).GetComponent<Image>().sprite = _raritySprites[_upgrades[i].Rarity.Name][1];
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
        SetUpgrades();
        UpdateUpgrades();
    }

    public void Scavenge()
    {
        
    }
}
