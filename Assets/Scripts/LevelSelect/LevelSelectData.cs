using System;
using System.Collections.Generic;
using System.Linq;
using LevelSelect;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace LevelSelect
{
    public enum LevelType
    {
        ENTRANCE,
        NORMAL,
        ELITE,
        BOSS,
        SPACE_STATION
        // TODO: warp (wormhole)
        // TODO: hidden?
        // TODO: other NPCs
        
    }
    
    public class LevelData
    {
        public LevelType Type;
        public int Difficulty;
        public int Waves;
        public Sprite Sprite;
        public List<int> Connections;

        public Vector3 WorldPosition;
    }
    
    public class LevelSelectData : ScriptableObject
    {
        [NonSerialized] private int _currentPlanet = -1;

        public int CurrentPlanet
        {
            get => _currentPlanet;
            set
            {
                VisitedPlanets.Add(value);
                _currentPlanet = value;
            }
        }
        [NonSerialized] public readonly List<int> VisitedPlanets = new();
        
        public LevelData[] Levels { get; private set; }
        public Tuple<int, int>[] Connections { get; private set; }
        
        public void PopulateData(LevelData[] levels, Tuple<int, int>[] connections)
        {
            Levels = levels;
            Connections = connections;

            VisitedPlanets.Clear();
            CurrentPlanet = 0;
        }

        public void ClearData()
        {
            _currentPlanet = -1;
            Levels = null;
            Connections = null;
        }
    }
}