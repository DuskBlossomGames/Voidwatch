using System;
using System.Collections.Generic;
using Menus;
using UnityEngine;

namespace LevelPlay
{
    public class HUDSizer : MonoBehaviour
    {
        public RectTransform healthBar, dashBar, scrapCount, minimap;

        private bool _hasScrap;

        private void Awake()
        {
            _hasScrap = scrapCount != null;
        }

        private readonly List<Vector2> _originals = new();

        private int _idx;
        private Vector2 GetOriginal(Vector2 value)
        {
            if (_originals.Count <= _idx) _originals.Add(value);
            return _originals[_idx++];
        }
        
        private void Update()
        {
            var scale = SettingsInterface.HUDSize;
            _idx = 0;
            
            healthBar.anchoredPosition = GetOriginal(healthBar.anchoredPosition) * scale;
            healthBar.localScale = GetOriginal(healthBar.localScale) * scale;

            dashBar.anchoredPosition = GetOriginal(dashBar.anchoredPosition) * scale;
            dashBar.localScale = GetOriginal(dashBar.localScale) * scale;

            if (_hasScrap)
            {
                scrapCount.anchoredPosition = GetOriginal(scrapCount.anchoredPosition) * scale;
                scrapCount.sizeDelta = GetOriginal(scrapCount.sizeDelta) * scale;
            }
            
            minimap.anchoredPosition = GetOriginal(minimap.anchoredPosition) * scale;
            minimap.sizeDelta = GetOriginal(minimap.sizeDelta) * scale;
        }
    }
}