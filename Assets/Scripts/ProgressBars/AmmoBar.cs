using System;
using UnityEngine;

namespace ProgressBars
{
    public class AmmoBar : ProgressBar
    {
        public Transform quadIIMask, quadIIIMask, quadIVMask;
        private Camera _cam;

        public void Start()
        {
            _cam = Camera.main;
        }
        
        private void Update()
        {
            transform.rotation = _cam.transform.rotation;
        }

        public override void UpdatePercentage(float cur, float max)
        {
            var empty = (1 - cur / max) * 270;

            print("rotating to: " + -empty);
            quadIVMask.RotateAround(transform.position, Vector3.forward, -Mathf.Min(empty, 90) - quadIVMask.localRotation.eulerAngles.z);
            quadIIIMask.RotateAround(transform.position, Vector3.forward, -Mathf.Min(empty, 180) - quadIIIMask.localRotation.eulerAngles.z);
            quadIIMask.RotateAround(transform.position, Vector3.forward, -empty - quadIIMask.localRotation.eulerAngles.z);
        }
    }
}