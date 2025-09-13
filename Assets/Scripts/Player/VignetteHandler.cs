using System;
using System.Collections.Generic;
using Q_Vignette.Scripts;
using UnityEngine;
using Util;

namespace Player
{
    [RequireComponent(typeof(Q_Vignette_Single))]
    public class VignetteHandler : MonoBehaviour
    {
        public AnimationCurve vignetteCurve;
        public float vignetteDuration, vignettePeak;
        
        private Q_Vignette_Single _vignette;
        private readonly Timer _vignetteTimer = new();
        private readonly List<float> _vignetteCacheKeys = new();
        private readonly List<float> _vignetteCacheValues = new();
        private float _vignettePeakAlpha;
        
        private void Awake()
        {
            _vignette = GetComponent<Q_Vignette_Single>();
            
            for (float t = 0; t <= vignettePeak * vignetteDuration; t += Time.fixedDeltaTime)
            {
                _vignetteCacheKeys.Add(vignetteCurve.Evaluate(t / vignetteDuration));
                _vignetteCacheValues.Add(vignetteDuration - t);
            }
        }
        
        private void FixedUpdate()
        {
            _vignetteTimer.FixedUpdate();
            _vignette.mainColor.a = _vignettePeakAlpha * vignetteCurve.Evaluate(1-_vignetteTimer.Progress);
        }

        public void Activate(float scale, float alpha)
        {
            if (_vignetteTimer.IsFinished)
            {
                _vignetteTimer.Value = vignetteDuration;
            } else if (1 - _vignetteTimer.Progress >= vignettePeak)
            {
                var curve = vignetteCurve.Evaluate(1 - _vignetteTimer.Progress);
                for (var i = 0; i < _vignetteCacheKeys.Count; i++) // pick the closest from before peak
                {
                    if (_vignetteCacheKeys[i] <= curve) continue;
                    
                    _vignetteTimer.SetValue(_vignetteCacheValues[i - 1]);
                    break;
                }
            }

            _vignette.mainScale = scale;
            _vignettePeakAlpha = alpha;
        }
    }
}