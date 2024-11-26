using System.Collections;
using System.Collections.Generic;
using LevelSelect;
using UnityEngine;
using Scriptable_Objects.Upgrades;

using static Static_Info.PlayerData;
public class UpgradeHandler : MonoBehaviour
{
    public FastDodgeCooldown defFastDodgeCooldown;

    public List<UpgradeInstance> upgrades;
    public List<BaseComponent> components;

    public TextTable cTable;
    public TextTable uTable;

    // Start is called before the first frame update
    void Start()
    {
        PlayerDataInstance.Upgrades = new List<UpgradeInstance>();
        if(true || PlayerDataInstance.Upgrades.Count == 0)
        {
            PlayerDataInstance.Upgrades.Clear();
            var nui = new UpgradeInstance()
            {
                upgrade = defFastDodgeCooldown,
                enabled = false,
                weaponID = null
            };
            PlayerDataInstance.Upgrades.Add(nui);
        }

        cTable.components = PlayerDataInstance.weapons;
        uTable.upgrades = PlayerDataInstance.Upgrades;

    }
}
