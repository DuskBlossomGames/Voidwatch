using System;
using UnityEngine;
using Util;

namespace Spawnables.Carcadon
{
    public class FadeToDeath : MonoBehaviour
    {
        public float fadeTime;
        
        [NonSerialized] public float TimeToLive;

        private SpriteRenderer _sr;
        private float _startAlpha;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            _startAlpha = _sr.color.a;
        }

        private void Update()
        {
            TimeToLive -= Time.deltaTime;

            _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, UtilFuncs.LerpSafe(0, _startAlpha, TimeToLive / fadeTime));
            if (TimeToLive <= 0) Destroy(gameObject);
        }
    }
}