using System;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Util
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FlashUI : MonoBehaviour
    {
        public float minAlpha, maxAlpha, time;

        private readonly Timer _timer = new();
        private int _dir = -1;
        private CanvasGroup _cg;
        
        private void OnEnable()
        {
            _cg = GetComponent<CanvasGroup>();
            _timer.Value = time;
            _timer.SetValue(Mathf.Clamp01((_cg.alpha-minAlpha)/(maxAlpha-minAlpha))*time);
        }

        private void Update()
        {
            _timer.Update(_dir);
            if (_timer.IsFinished) _dir = 1;
            if (_timer.Progress == 1) _dir = -1;
            
            _cg.alpha = Mathf.SmoothStep(minAlpha, maxAlpha, _timer.Progress);
        }
    }
}