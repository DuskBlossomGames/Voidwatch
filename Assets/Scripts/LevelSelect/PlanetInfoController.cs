using System;
using System.Collections;
using System.Linq;
using Extensions;
using Singletons.Static_Info;
using TMPro;
using UnityEngine;
using Util;
using static Singletons.Static_Info.LevelSelectData;

namespace LevelSelect
{
    public class PlanetInfoController : MonoBehaviour
    {
        public Selector selector;
        public float yOffset;
        public float fadeTime;

        [NonSerialized] public PlanetController SelectedPlanet;
        
        private TextMeshPro _title, _description, _difficulty, _travel, _shift;
        private SpriteRenderer _background;

        private LevelData _level;
        
        private readonly Timer _fadeTimer = new();
        private bool _shown;

        private void Awake()
        {
            _fadeTimer.Value = fadeTime;
            _fadeTimer.SetValue(0);

            _title = GetComponentsInChildren<TextMeshPro>()[0];
            _description = GetComponentsInChildren<TextMeshPro>()[1];
            _difficulty = GetComponentsInChildren<TextMeshPro>()[2];
            _travel = GetComponentsInChildren<TextMeshPro>()[3];
            _shift = GetComponentsInChildren<TextMeshPro>()[4];
            _background = GetComponent<SpriteRenderer>();
            
            selector.OnSelectionChange += pos =>
            {
                _shown = pos != null;
                if (!_shown) return;
                
                transform.position = pos!.Value + yOffset * Vector3.up;

                _level = LevelSelectDataInstance.Levels.First(l => l.WorldPosition == pos);
                _title.text = _level.Title;
                _description.text = _level.Description;
                _difficulty.text = _level.Difficulty.Text;
                _difficulty.color = _level.Difficulty.Color;
                _travel.text = _level.Travellable ? "Click to Travel" : "Cannot Travel";
                _travel.color = Color.HSVToRGB(0, 0, _level.Travellable ? 1 : 0.75f);
            };
        }
        private void Update()
        {
            _fadeTimer.Update(_shown ? 1 : -1);
            _title.SetAlpha(_fadeTimer.Progress);
            _description.SetAlpha(_fadeTimer.Progress);
            _difficulty.SetAlpha(_fadeTimer.Progress);
            _background.SetAlpha(_fadeTimer.Progress);
            _travel.SetAlpha(_fadeTimer.Progress);
            _shift.SetAlpha(_fadeTimer.Progress);
        }

        private void OnMouseUpAsButton()
        {
            SelectedPlanet.OnMouseUpAsButton();
        }
    }
}