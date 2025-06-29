using System;
using Singletons.Static_Info;
using UnityEngine;
using static Singletons.Static_Info.LevelSelectData;
using Timer = Util.Timer;

namespace LevelSelect
{
    public class PlanetController : MonoBehaviour
    {
        public float maxScaleMult;
        public MiniPlayerController playerMini;
        
        [NonSerialized] public int LevelIdx;
        [NonSerialized] public bool Clickable;
        
        private LevelData Level => LevelSelectDataInstance.Levels[LevelIdx];

        private bool _selected;
        
        private float _baseScale;
        private void Start()
        {
            _baseScale = transform.localScale.x;
            _scaleTimer.Value = 1;
            _scaleTimer.SetValue(0);
            
            if (Clickable && Level.Type != LevelType.SpaceStation) transform.GetChild(0).gameObject.SetActive(true);
            GetComponent<Selectable>().clickable = Clickable;
            GetComponent<Selectable>().selector.OnSelectionChange += pos => _selected = pos == Level.WorldPosition;
        }

        private float _scaleMult = 1;
        private readonly Timer _scaleTimer = new();
        private int _dir = 1;
        private void Update()
        {
            if (!Clickable) return;

            _scaleTimer.Update(_dir);
            if (_scaleTimer.Value >= 1) _dir = -1;
            if (_scaleTimer.Value <= 0) _dir = 1;
            
            _scaleMult = Mathf.SmoothStep(1, maxScaleMult, _scaleTimer.Value);
            transform.localScale = new Vector3(_baseScale * _scaleMult, _baseScale * _scaleMult, 1);
        }

        private void OnMouseUpAsButton()
        {
            if (!Clickable || !_selected) return;

            foreach (var s in transform.parent.GetComponentsInChildren<Selectable>()) s.clickable = false;
            
            playerMini.GoTo(Level.WorldPosition, LevelIdx,
                Level.IsBoss ? "LevelBoss" : Level.Type == LevelType.SpaceStation ? "Shop" : "LevelPlay");
        }

    }
}