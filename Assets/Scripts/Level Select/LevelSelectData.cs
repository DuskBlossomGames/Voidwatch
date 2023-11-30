using System;
using System.Collections.Generic;
using UnityEngine;

namespace Level_Select
{
    public class LevelData
    {
        public int Difficulty;
        public Sprite Sprite;

        public Vector3 WorldPosition;
    }
    
    public class LevelSelectData : ScriptableObject
    {
        [NonSerialized] public int CurrentPlanet = -1;
        [NonSerialized] public List<int> VisitedPlanets;
        
        public LevelData[] Levels { get; private set; }
        public Tuple<int, int>[] Connections { get; private set; }

        public void PopulateData(LevelData[] levels, Tuple<int, int>[] connections)
        {
            Levels = levels;
            Connections = connections;
            VisitedPlanets = CurrentPlanet != -1 ? new List<int> { CurrentPlanet } : new List<int>();
            
            OnPopulate?.Invoke();
        }
        
        public event Action OnPopulate;
    }
}