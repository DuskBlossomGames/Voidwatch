using System;
using Extensions;
using UnityEngine;

namespace Util
{
    public class SpriteFadeToDeath : MonoBehaviour
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

            _sr.SetAlpha(UtilFuncs.LerpSafe(0, _startAlpha, TimeToLive / fadeTime));
            if (TimeToLive <= 0) Destroy(gameObject);
        }
    }
}