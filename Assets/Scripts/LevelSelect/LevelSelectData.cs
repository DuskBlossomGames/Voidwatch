using System;
using System.Collections.Generic;
using System.Linq;
using LevelSelect;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace LevelSelect
{
    public class LevelType
    {
        public static readonly LevelType Entrance = new("The entrance to the galaxy. Where you just came from!");
        public static readonly LevelType Normal = new("The Cult of the Void has control. Can you break them?");
        public static readonly LevelType Elite = new("The Cult of the Void has control. Can you bre~kskxzsh");
        public static readonly LevelType Boss = new("The Void beckons...");
        public static readonly LevelType SpaceStation = new("A temporary respite for travelers. Who might you find?");

        public readonly string Description;

        private LevelType() { }
        private LevelType(string description)
        {
            Description = description;
        }
        
        // TODO: warp (wormhole)
        // TODO: hidden?
        // TODO: other NPCs

    }
    
    public class LevelData
    {
        public LevelType Type;
        public int Difficulty;
        public int Loot;
        public int Waves;
        public Sprite Sprite;
        public List<int> Connections;

        private Sprite _hiddenSprite;
        public Sprite HiddenSprite
        {
            get => _hiddenSprite ?? Sprite;
            set => _hiddenSprite = value;
        }

        public Vector3 WorldPosition;

        public string Name;
        public string LoreText;
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