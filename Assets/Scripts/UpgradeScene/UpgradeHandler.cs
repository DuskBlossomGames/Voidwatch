using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scriptable_Objects.Upgrades;

public class UpgradeHandler : MonoBehaviour
{
    public FastDodgeCooldown defFastDodgeCooldown;
    public Scriptable_Objects.PlayerData playerData;

    public List<UpgradeInstance> upgrades;
    public List<BaseComponent> components;

    public TextTable cTable;
    public TextTable uTable;

    // Start is called before the first frame update
    void Start()
    {
        playerData.upgrades = new List<UpgradeInstance>();
        if(true || playerData.upgrades.Count == 0)
        {
            playerData.upgrades.Clear();
            var nui = new UpgradeInstance()
            {
                upgrade = defFastDodgeCooldown,
                enabled = false,
                weaponID = null
            };
            playerData.upgrades.Add(nui);
        }

        cTable.components = playerData.weapons;
        uTable.upgrades = playerData.upgrades;

    }
}
