using System.Collections;
using System.Linq;
using Extensions;
using TMPro;
using UnityEngine;
using Util;
using static Singletons.Static_Info.LevelSelectData;

namespace LevelSelect
{
    public class PlanetInfoController : MonoBehaviour
    {
        public Selector selector;
        public float maxScaleMult;
        public float yOffset;
        public float fadeTime;

        private TextMeshPro _title, _description, _difficulty;
        private SpriteRenderer _background;


        private float _baseScale;
        private float _scaleMult = 1;
        private readonly Timer _scaleTimer = new();
        private int _dir = 1;

        private readonly Timer _fadeTimer = new();
        private bool _shown;

        private void Awake()
        {
            _baseScale = transform.localScale.x;
            _scaleTimer.Value = 1;
            _scaleTimer.SetValue(0);
            
            _fadeTimer.Value = fadeTime;
            _fadeTimer.SetValue(0);

            _title = GetComponentsInChildren<TextMeshPro>()[0];
            _description = GetComponentsInChildren<TextMeshPro>()[1];
            _difficulty = GetComponentsInChildren<TextMeshPro>()[2];
            _background = GetComponent<SpriteRenderer>();
            
            selector.OnSelectionChange += pos =>
            {
                _shown = pos != null;
                if (!_shown) return;
                
                transform.position = pos!.Value + yOffset * Vector3.up;

                var level = LevelSelectDataInstance.Levels.First(l => l.WorldPosition == pos);
                _title.text = level.Title;
                _description.text = level.Description;
                _difficulty.text = level.Difficulty.Text;
                _difficulty.color = level.Difficulty.Color;
            };
        }
        private void Update()
        {
            _fadeTimer.Update(_shown ? 1 : -1);
            _title.SetAlpha(_fadeTimer.Progress);
            _description.SetAlpha(_fadeTimer.Progress);
            _difficulty.SetAlpha(_fadeTimer.Progress);
            _background.SetAlpha(_fadeTimer.Progress);
            
            _scaleTimer.Update(_dir);
            if (_scaleTimer.Value >= 1) _dir = -1;
            if (_scaleTimer.Value <= 0) _dir = 1;
            
            _scaleMult = Mathf.SmoothStep(1, maxScaleMult, _scaleTimer.Value);
            transform.localScale = new Vector3(_baseScale * _scaleMult, _baseScale * _scaleMult, 1);
        }
    }
}