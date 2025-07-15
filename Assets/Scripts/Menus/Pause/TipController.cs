using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Menus.Pause
{
    public class TipController : MonoBehaviour
    {
        private static readonly string[] Tips = {
            "All keybinds can be changed in the settings menu.",
            "When exiting a dash, previous velocity is resumed in the ship's new direction.",
            "If you press <b>Dash</b> again partway through, the dash will be redirected and last longer.",
            "Enemies killed by collision damage do not injure your ship.",
            "When ammo is fully depleted, it takes longer to refill.",
            "When your shield is broken, it takes longer to recharge.",
            "When orbiting a planet, your bullets appear to curve due to the coriolis effect.",
            "Platelin colonies will multiply to be a problem if not dealt with early.",
            "Chargers can be shot down to prevent their EMP detonation.",
            "Asteroids can be deadly!",
            "At space stations, you can boost Health, Shield, Speed, Void Energy, Damage, and Ammo."
        };

        public float thickness, padding;

        private string _format;

        private List<string> _validTips = new(Tips);
        
        private static TextMeshProUGUI _text;
        private RectTransform _self, _left, _right, _top, _bottom;
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _self = GetComponent<RectTransform>();
            _left = transform.GetChild(0).GetComponent<RectTransform>();
            _right = transform.GetChild(1).GetComponent<RectTransform>();
            _top = transform.GetChild(2).GetComponent<RectTransform>();
            _bottom = transform.GetChild(3).GetComponent<RectTransform>();

            _format = _text.text;
        }

        private void OnEnable()
        {
            var tip = _validTips[Random.Range(0, _validTips.Count)];
            _validTips.Remove(tip); // ensure we go through every tip before cycling
            if (_validTips.Count == 0)
            {
                _validTips.AddRange(Tips);
                _validTips.Remove(tip); // don't allow back-to-back
            }
            _text.text = string.Format(_format, tip);

            _text.ForceMeshUpdate();
            _text.mesh.RecalculateBounds();

            var width = _text.mesh.bounds.size.x;
            var height = _text.preferredHeight + _text.margin.y;

            var left = -padding / _self.rect.width;
            var right = (width + padding) / _self.rect.width;
            var top = 1 + padding / _self.rect.height;
            var bottom = 1 - (height + padding) / _self.rect.height;

            _left.sizeDelta = new Vector2(thickness, height + padding * 2);
            _left.anchorMin = _left.anchorMax = new Vector2(left, top);
            
            _right.sizeDelta = new Vector2(thickness, height + padding * 2);
            _right.anchorMin = _right.anchorMax = new Vector2(right, top);
            
            _top.sizeDelta = new Vector2(width + padding * 2 + thickness, thickness);
            _top.anchorMin = _top.anchorMax = new Vector2(left - thickness/2 / _self.rect.width, top);
            
            _bottom.sizeDelta = new Vector2(width + padding * 2 + thickness, thickness);
            _bottom.anchorMin = _bottom.anchorMax = new Vector2(left - thickness/2 / _self.rect.width, bottom);
        }
    }
}