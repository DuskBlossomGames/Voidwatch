using UnityEngine;

namespace Util
{
    public class ScaleListener : MonoBehaviour
    {
        public float maxMaxScale;
        
        private float _baseScale;
        private float _scaleMult = 1;

        private void Start()
        {
            _baseScale = transform.localScale.x;
        }

        public void SetScaleMult(float scaleMult) => _scaleMult = scaleMult;
        
        public void SetScale(float progress, float maxScale)
        {
            maxScale = Mathf.Min(maxScale, maxMaxScale);
            maxScale = 1 + (maxScale - 1) / _scaleMult;
            var scale = Mathf.SmoothStep(1, maxScale, progress) * _baseScale * _scaleMult;
            
            transform.localScale = new Vector3(scale, scale, 1);
        }
    }
}