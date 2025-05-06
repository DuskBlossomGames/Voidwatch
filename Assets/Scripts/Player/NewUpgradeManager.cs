using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using Upgrade = UpgradePlayer.Upgrades;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;

public class NewUpgradeManager : MonoBehaviour
{
    public GameObject upgradeHolder; // TODO: deprecated
    public RawImage minimap;
    public RectTransform titleBox, title, subtitle;
    public CanvasGroup everythingElse;
    public float minimapFadeTime, titleTime, titleWaitTime, subtitleFadeTime, subtitleWaitTime, slideTime, slideWaitTime, fadeInTime;
    public float startAnchorMin, startAnchorMax;
    
    public Player.Movement playMov;
    public Player.FollowPlayer followPlayer;
    private GameObject _left;
    private GameObject _center;
    private GameObject _right;
    private Upgrade[] _upgrades = new Upgrade[3];

    public void Start()
    {
        _center = upgradeHolder.transform.GetChild(0).gameObject;
        _left = upgradeHolder.transform.GetChild(1).gameObject;
        _right = upgradeHolder.transform.GetChild(2).gameObject;
    }
    
    public void Show()
    {
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
        everythingElse.gameObject.SetActive(true);
        for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
        {
            everythingElse.alpha = t/fadeInTime;
            yield return new WaitForFixedUpdate();
        }
        
        // var allUps = System.Enum.GetValues(typeof(Upgrade));
        // upgradeHolder.SetActive(true);
        // _upgrades[0] = (Upgrade)allUps.GetValue(Random.Range(0, allUps.Length));
        // do _upgrades[1] = (Upgrade)allUps.GetValue(Random.Range(0, allUps.Length));
        //     while (_upgrades[1] == _upgrades[0]);
        // do _upgrades[2] = (Upgrade)allUps.GetValue(Random.Range(0, allUps.Length));
        //     while (_upgrades[2] == _upgrades[1] || _upgrades[2] == _upgrades[0]);
        //
        // _left.transform.GetChild(0).GetComponent<TMP_Text>().text = UpgradePlayer.UpName(_upgrades[0]);
        // _left.transform.GetChild(1).GetComponent<TMP_Text>().text = UpgradePlayer.UpBody(_upgrades[0]);
        // _center.transform.GetChild(0).GetComponent<TMP_Text>().text = UpgradePlayer.UpName(_upgrades[1]);
        // _center.transform.GetChild(1).GetComponent<TMP_Text>().text = UpgradePlayer.UpBody(_upgrades[1]);
        // _right.transform.GetChild(0).GetComponent<TMP_Text>().text = UpgradePlayer.UpName(_upgrades[2]);
        // _right.transform.GetChild(1).GetComponent<TMP_Text>().text = UpgradePlayer.UpBody(_upgrades[2]);
        //
    }

    public void Select(int i)
    {
        UpgradePlayer.Upgrade(_upgrades[i]);
        SceneManager.LoadScene("LevelSelect");
    }
}
