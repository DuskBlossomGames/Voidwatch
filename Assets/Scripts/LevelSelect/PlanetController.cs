using System;
using Singletons.Static_Info;
using UnityEngine;
using Util;
using static Singletons.Static_Info.LevelSelectData;
using Timer = Util.Timer;

namespace LevelSelect
{
    public class PlanetController : MonoBehaviour
    {
        public MiniPlayerController playerMini;
        public ScaleListener infoScaleListener;
        
        [NonSerialized] public int LevelIdx;
        [NonSerialized] public bool Clickable;
        
        private LevelData Level => LevelSelectDataInstance.Levels[LevelIdx];

        private bool _selected;
        
        private void Start()
        {
            if (Level.Travellable && Level.Type == LevelType.Normal) transform.GetChild(0).gameObject.SetActive(true);

            var scaleUi = GetComponent<ScaleUI>();
            var pic = infoScaleListener.GetComponent<PlanetInfoController>();
            var selector = GetComponent<Selectable>().selector;
            GetComponent<Selectable>().clickable = Clickable;
            GetComponent<Selectable>().selector.OnSelectionChange += pos =>
            {
                _selected = pos == Level.WorldPosition;
                if (_selected)
                {
                    pic.SelectedPlanet = this;
                    selector.SetScaleMult(Level.SpriteData.RadiusMult);
                    scaleUi.listeners.Add(infoScaleListener);
                }
                else
                {
                    scaleUi.listeners.Remove(infoScaleListener);
                }
            };
        }

        public void OnMouseUpAsButton()
        {
            if (!Level.Travellable || !_selected) return;
            GetComponent<Selectable>().selector.SetPosition(null); // deselect

            foreach (var s in transform.parent.GetComponentsInChildren<Selectable>()) s.clickable = false;
            
            playerMini.GoTo(Level.WorldPosition, LevelIdx,
                Level.Type switch
                {
                    LevelType.Boss => "LevelBoss",
                    LevelType.SpaceStation => "Shop",
                    LevelType.Tutorial => "Tutorial",
                    _ => "LevelPlay"
                });
        }
    }
}