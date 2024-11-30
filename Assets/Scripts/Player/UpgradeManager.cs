using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewUpgradeManager : MonoBehaviour
{
    public GameObject upgradeHolder;
    public Player.Movement playMov;
    private GameObject _left;
    private GameObject _center;
    private GameObject _right;

    public void Start()
    {
        _center = upgradeHolder.transform.GetChild(0).gameObject;
        _left = upgradeHolder.transform.GetChild(1).gameObject;
        _right = upgradeHolder.transform.GetChild(2).gameObject;
    }
}
