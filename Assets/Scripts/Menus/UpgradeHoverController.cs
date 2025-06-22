using System;
using Extensions;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Util;
using static Static_Info.PlayerData;

namespace Menus
{
    public class UpgradeHoverController : MonoBehaviour
    {
        public float fadeTime;
        public float anchorSpacing;
        
        private float _width;
        
        private UpgradePlayer.Upgrade _upgrade;
        private int _idx = -1;
        private float _horizCenter;
        private bool _reOpen;

        private RectTransform _rect;
        private Image[] _images;
        private TextMeshProUGUI[] _text;

        private readonly Timer _fade = new();
        private int _dir = -1;

        private void Awake()
        {
            _fade.Value = fadeTime;
            _fade.SetValue(0);
            
            _rect = GetComponent<RectTransform>();
            _images = GetComponentsInChildren<Image>(true);
            _text = GetComponentsInChildren<TextMeshProUGUI>(true);
            
            _width = _rect.anchorMax.x - _rect.anchorMin.x;;
        }

        private void OnDisable()
        {
            _upgrade = null;
            _idx = -1;

            _fade.SetValue(0);
            foreach (var image in _images) image.SetAlpha(0);
            foreach (var text in _text) text.SetAlpha(0);
        }

        private void Update()
        {
            _fade.CustomUpdate(Time.unscaledDeltaTime, _dir);

            foreach (var image in _images) image.SetAlpha(_fade.Progress);
            foreach (var text in _text) text.SetAlpha(_fade.Progress);

            if (_fade.IsFinished && _reOpen)
            {
                _reOpen = false;
                StartOpening();
            }
        }

        private void StartOpening()
        {
            _dir = 1;
            
            _text[0].text = _upgrade.Title;
            _text[1].text = _upgrade.Description;
            _text[2].text = _upgrade.Quip;

            _images[0].sprite = PlayerDataInstance.RaritySprites[_upgrade.Rarity.Name][1];
            
            _rect.anchorMax = new Vector2(_horizCenter + _width / 2, _rect.anchorMax.y);
            _rect.anchorMin = new Vector2(_horizCenter - _width / 2, _rect.anchorMin.y);
        }
        
        public void PointerEntered(UpgradePlayer.Upgrade upgrade, int idx, float center)
        {
            center = Mathf.Clamp(center, anchorSpacing + _width / 2, 1 - anchorSpacing - _width / 2);
            
            if (_idx == idx)
            {
                StartOpening();
                return;
            }

            _idx = idx;
            _upgrade = upgrade;
            _horizCenter = center;
            if (_fade.IsFinished) StartOpening();
            else _reOpen = true;
        }

        public void PointerExited()
        {
            _dir = -1;
        }
    }
}