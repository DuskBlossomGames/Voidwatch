using System;
using System.Collections;
using Spawnables;
using Spawnables.Player;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Bosses.Worm
{
    public class BlackHolePulse : MonoBehaviour
    {
        public PlayerDamageable player;
        public AnimationCurve distortion;
        public PostProcessVolume postProcess;
        public float distortionTime, distortionScale;
        public float expandSpeed, expandDistance;
        public float damage, lrWidth;
        public int numPoints;

        private LensDistortion _ld;
        private LineRenderer _lr;
        private Collider2D _playerCollider;

        private void Start()
        {
            _ld = postProcess.profile.GetSetting<LensDistortion>();
            _lr = GetComponentInChildren<LineRenderer>();
            _playerCollider = player.GetComponent<Collider2D>();

            _lr.startWidth = _lr.endWidth = lrWidth;
            _lr.positionCount = numPoints;
        }

        private bool _pulsing;
        public IEnumerator Pulse()
        {
            if (_pulsing) yield break;
            _pulsing = true;
            
            for (float t = 0; t < distortionTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                _ld.intensity.value = distortionScale * distortion.Evaluate(t / distortionTime);
            }

            _lr.enabled = true;
            for (float t = 0; t < expandDistance / expandSpeed; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();

                var radius = expandSpeed * t;
                
                var points = new Vector3[numPoints];
                for (var i = 0; i < numPoints; i++)
                {
                    var angle = Mathf.PI * 2f * i / (numPoints-1);
                    points[i] = radius * new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                }
                _lr.SetPositions(points);

                var playerRad = ((Vector2)player.transform.position).magnitude;
                if (_playerCollider.enabled && Mathf.Abs(playerRad - radius) < lrWidth) player.Damage(damage, IDamageable.DmgType.Concussive, gameObject);
            }
            _lr.enabled = false;

            _pulsing = false;
        }
    }
}