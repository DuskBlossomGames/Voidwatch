using UnityEngine;

namespace Util
{
    public class ScaleListener : MonoBehaviour
    {
        public float maxMaxScale;
        
        private float _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale.x;
        }

        public void SetScale(float progress, float maxScale)
        {
            maxScale = Mathf.Min(maxScale, maxMaxScale);
            var scale = Mathf.SmoothStep(1, maxScale, progress);
            
            transform.localScale = new Vector3(_baseScale * scale, _baseScale * scale, 1);
        }
    }
}