using System;
using UnityEngine;

namespace Level_Select
{
    public class LevelData
    {
        public int Difficulty;
        public Sprite Sprite;
        public Vector2 Position;
    }
    
    public class LevelSelectData : ScriptableObject
    {
        [NonSerialized] public int CurrentPlanet = -1;
        public LevelData[] Levels;
        public Tuple<int, int>[] Connections;
    }
}