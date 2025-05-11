using UnityEngine;

namespace ProgressBars
{
    public class HealthBar : ProgressBar
    {
        public float radius;
        public AnimationCurve opacityCurve;

        private Camera _camera;
        private SpriteRenderer[] _srs;
        private float _opacityPos = 1;

        private void Awake()
        {
            _srs = GetComponentsInChildren<SpriteRenderer>();
            _camera = Camera.main;
        }

        private void Update()
        {
            _opacityPos = Mathf.Clamp01(_opacityPos - 0.7f * Time.deltaTime);
            foreach (var sr in _srs)
            {
                var color = sr.color;
                color.a = opacityCurve.Evaluate(_opacityPos); // magic constant: speed
                sr.color = color;
            }

            var camAngle = _camera.transform.eulerAngles.z * Mathf.Deg2Rad + Mathf.PI/2;
            transform.position = transform.parent.position + radius * new Vector3(Mathf.Cos(camAngle) / transform.parent.lossyScale.x,
                Mathf.Sin(camAngle) / transform.parent.lossyScale.y, 0);
            transform.rotation = _camera.transform.rotation;
        }

        public override void UpdatePercentage(float cur, float max)
        {
            base.UpdatePercentage(cur, max);

            if (_opacityPos == 0) _opacityPos = 1;
            else _opacityPos = Mathf.Max(_opacityPos, 0.9f); // magic constant: peak
        }
    }
}