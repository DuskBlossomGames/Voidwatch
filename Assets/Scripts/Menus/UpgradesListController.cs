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
        public float sideSpace;
        
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

            var right = transform.GetChild(1).GetChild(1).GetComponent<RectTransform>().anchorMax.x
                + 2 * (_prefab.anchorMin.x - transform.GetChild(1).GetChild(1).GetComponent<RectTransform>().anchorMax.x)
                + spacing * (PlayerDataInstance.Upgrades.Count-1)
                + _prefab.anchorMax.x-_prefab.anchorMin.x;
            transform.GetChild(1).gameObject.SetActive(PlayerDataInstance.Upgrades.Count > 0);
            transform.GetChild(1).GetChild(2).GetComponent<RectTransform>().anchorMin = new Vector2(right, 0);
            transform.GetChild(1).GetChild(2).GetComponent<RectTransform>().anchorMax = new Vector2(right, 1);
            transform.GetChild(1).GetChild(3).GetComponent<RectTransform>().anchorMax = new Vector2(right, 1);
            transform.GetChild(1).GetChild(4).GetComponent<RectTransform>().anchorMax = new Vector2(right, 0);
        }

        private void OnDisable()
        {
            for (var i = 3; i < transform.childCount; i++) Destroy(transform.GetChild(i).gameObject);
        }
    }
}