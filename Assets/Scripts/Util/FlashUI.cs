using System;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    public class FlashUI : MonoBehaviour
    {
        public float minAlpha, maxAlpha, time;

        private readonly Timer _timer = new();
        private int _dir = -1;
        private Image[] _images;
        private TextMeshProUGUI[] _text;
        
        private void Awake()
        {
            _timer.Value = time;
            _timer.SetValue(0);
            _images = GetComponentsInChildren<Image>();
            _text = GetComponentsInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            _timer.Update(_dir);
            if (_timer.IsFinished) _dir = 1;
            if (_timer.Progress == 1) _dir = -1;
            
            foreach (var image in _images) image.SetAlpha(Mathf.SmoothStep(minAlpha, maxAlpha, _timer.Progress));
            foreach (var text in _text) text.SetAlpha(Mathf.SmoothStep(minAlpha, maxAlpha, _timer.Progress));
        }
    }
}