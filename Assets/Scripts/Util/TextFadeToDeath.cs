using System;
using Extensions;
using TMPro;
using UnityEngine;

namespace Util
{
    public class TextFadeToDeath : MonoBehaviour
    {
        public float fadeTime;
        public float timeToLive;

        private TextMeshPro _text;
        private float _startAlpha;

        private void Start()
        {
            _text = GetComponent<TextMeshPro>();
            _startAlpha = _text.color.a;
        }

        private void Update()
        {
            timeToLive -= Time.deltaTime;

            _text.SetAlpha(UtilFuncs.LerpSafe(0, _startAlpha, timeToLive / fadeTime));
            if (timeToLive <= 0) Destroy(gameObject);
        }
    }
}