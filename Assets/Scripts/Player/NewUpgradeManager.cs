using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Upgrade = UpgradePlayer.Upgrades;
using UnityEngine.SceneManagement;

public class NewUpgradeManager : MonoBehaviour
{
    public GameObject upgradeHolder;
    public Player.Movement playMov;
    public Player.FollowPlayer followPlayer;
    private GameObject _left;
    private GameObject _center;
    private GameObject _right;
    private bool _show = false;
    private Upgrade[] _upgrades = new Upgrade[3];

    public void Start()
    {
        _center = upgradeHolder.transform.GetChild(0).gameObject;
        _left = upgradeHolder.transform.GetChild(1).gameObject;
        _right = upgradeHolder.transform.GetChild(2).gameObject;
    }

    public void Update()
    {
        upgradeHolder.SetActive(_show);
    }

    public void Show()
    {
        var allUps = System.Enum.GetValues(typeof(Upgrade));
        _show = true;
        _upgrades[0] = (Upgrade)allUps.GetValue(Random.Range(0, allUps.Length));
        do _upgrades[1] = (Upgrade)allUps.GetValue(Random.Range(0, allUps.Length));
            while (_upgrades[1] == _upgrades[0]);
        do _upgrades[2] = (Upgrade)allUps.GetValue(Random.Range(0, allUps.Length));
            while (_upgrades[2] == _upgrades[1] || _upgrades[2] == _upgrades[0]);

        _left.transform.GetChild(0).GetComponent<TMP_Text>().text = UpgradePlayer.UpName(_upgrades[0]);
        _left.transform.GetChild(1).GetComponent<TMP_Text>().text = UpgradePlayer.UpBody(_upgrades[0]);
        _center.transform.GetChild(0).GetComponent<TMP_Text>().text = UpgradePlayer.UpName(_upgrades[1]);
        _center.transform.GetChild(1).GetComponent<TMP_Text>().text = UpgradePlayer.UpBody(_upgrades[1]);
        _right.transform.GetChild(0).GetComponent<TMP_Text>().text = UpgradePlayer.UpName(_upgrades[2]);
        _right.transform.GetChild(1).GetComponent<TMP_Text>().text = UpgradePlayer.UpBody(_upgrades[2]);

        playMov.SetInputBlocked(true);
        playMov.autoPilot = true;
        playMov.gameObject.GetComponent<Spawnables.Player.PlayerDamageable>().godmode = true;
        followPlayer.suppres = true;
    }

    public void Select(int i)
    {
        UpgradePlayer.Upgrade(_upgrades[i]);
        SceneManager.LoadScene("LevelSelect");
    }
}
