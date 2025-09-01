using System;
using System.Collections.Generic;
using UnityEngine;

namespace Util
{
    public class ScaleUI : MonoBehaviour
    {
        public float maxScaleMult;
        public float time = 1;

        [NonSerialized] public readonly List<ScaleListener> listeners = new(); 
        
        private float _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale.x;
            _scaleTimer.Value = time;
            _scaleTimer.SetValue(0);
        }
        
        private readonly Timer _scaleTimer = new();
        private int _dir = 1;
        private void Update()
        {
            _scaleTimer.Update(_dir);
            if (_scaleTimer.Value >= time) _dir = -1;
            if (_scaleTimer.Value <= 0) _dir = 1;
            
            var scaleMult = Mathf.SmoothStep(1, maxScaleMult, _scaleTimer.Progress);
            transform.localScale = new Vector3(_baseScale * scaleMult, _baseScale * scaleMult, 1);
            
            listeners.ForEach(l => l.SetScale(_scaleTimer.Progress, maxScaleMult));
        }

    }
}