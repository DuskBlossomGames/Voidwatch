using System;
using Singletons.Static_Info;
using UnityEngine;
using static Singletons.Static_Info.LevelSelectData;
using Timer = Util.Timer;

namespace LevelSelect
{
    public class PlanetController : MonoBehaviour
    {
        public MiniPlayerController playerMini;
        
        [NonSerialized] public int LevelIdx;
        [NonSerialized] public bool Clickable;
        
        private LevelData Level => LevelSelectDataInstance.Levels[LevelIdx];

        private bool _selected;
        
        private void Start()
        {
            if (Clickable && Level.Type == LevelType.Normal) transform.GetChild(0).gameObject.SetActive(true);
            GetComponent<Selectable>().clickable = Clickable;
            GetComponent<Selectable>().selector.OnSelectionChange += pos => _selected = pos == Level.WorldPosition;
        }

        private void OnMouseUpAsButton()
        {
            if (!Clickable || !_selected) return;

            foreach (var s in transform.parent.GetComponentsInChildren<Selectable>()) s.clickable = false;
            
            playerMini.GoTo(Level.WorldPosition, LevelIdx,
                Level.Type == LevelType.Boss ? "LevelBoss" : Level.Type == LevelType.SpaceStation ? "Shop" : Level.Type == LevelType.Tutorial ? "Tutorial" : "LevelPlay");
        }

    }
}