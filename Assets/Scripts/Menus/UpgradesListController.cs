using System;
using System.Collections.Generic;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using static Static_Info.PlayerData;

namespace Menus
{
    public class UpgradesListController : MonoBehaviour
    {
        public float spacing;
        
        private RectTransform _prefab;
        private void Awake()
        {
            _prefab = transform.GetChild(0).GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            for (var i = 0; i < PlayerDataInstance.Upgrades.Count; i++)
            {
                var upgrade = Instantiate(_prefab.gameObject, transform).GetComponent<RectTransform>();
                
                upgrade.anchorMin = new Vector2(_prefab.anchorMin.x + spacing*i, _prefab.anchorMin.y);
                upgrade.anchorMax = new Vector2(_prefab.anchorMax.x + spacing*i, _prefab.anchorMax.y);
                
                upgrade.GetComponent<UpgradeController>().Upgrade = PlayerDataInstance.Upgrades[i];
                
                var images = upgrade.GetComponentsInChildren<Image>();
                images[1].sprite = PlayerDataInstance.UpgradeSprites[PlayerDataInstance.Upgrades[i].Title];
                images[2].sprite = PlayerDataInstance.RaritySprites[PlayerDataInstance.Upgrades[i].Rarity.Name][0];

                upgrade.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            for (var i = 2; i < transform.childCount; i++) Destroy(transform.GetChild(i));
        }
    }
}