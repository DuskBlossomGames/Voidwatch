using UnityEngine;

namespace Util
{
    public class ScaleUI : MonoBehaviour
    {
        public float maxScaleMult;
        public float time = 1;

        private float _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale.x;
            _scaleTimer.Value = time;
            _scaleTimer.SetValue(0);
        }
        
        private float _scaleMult = 1;
        private readonly Timer _scaleTimer = new();
        private int _dir = 1;
        private void Update()
        {
            _scaleTimer.Update(_dir);
            if (_scaleTimer.Value >= time) _dir = -1;
            if (_scaleTimer.Value <= 0) _dir = 1;
            
            _scaleMult = Mathf.SmoothStep(1, maxScaleMult, _scaleTimer.Progress);
            transform.localScale = new Vector3(_baseScale * _scaleMult, _baseScale * _scaleMult, 1);
        }

    }
}