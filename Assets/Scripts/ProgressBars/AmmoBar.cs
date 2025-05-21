using UnityEngine;

namespace ProgressBars
{
    public class AmmoBar : ProgressBar
    {
        public Transform quadIIMask, quadIIIMask, quadIVMask;
        public AnimationCurve opacityCurve;

        private SpriteRenderer[] _srs;
        private Camera _cam;
        private float _opacityPos = 0;

        public void Start()
        {
            _cam = Camera.main;
            _srs = GetComponentsInChildren<SpriteRenderer>();
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

            transform.rotation = _cam.transform.rotation;
        }

        public override void UpdatePercentage(float cur, float max)
        {
            if (_opacityPos == 0) _opacityPos = 1;
            else _opacityPos = Mathf.Max(_opacityPos, 0.9f); // magic constant: peak
            
            var empty = (1 - cur / max) * 270;

            quadIVMask.RotateAround(transform.position, Vector3.forward, -Mathf.Min(empty, 90) - quadIVMask.localRotation.eulerAngles.z);
            quadIIIMask.RotateAround(transform.position, Vector3.forward, -Mathf.Min(empty, 180) - quadIIIMask.localRotation.eulerAngles.z);
            quadIIMask.RotateAround(transform.position, Vector3.forward, -empty - quadIIMask.localRotation.eulerAngles.z);
        }
    }
}
