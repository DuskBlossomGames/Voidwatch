using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Player.Upgrades;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Singletons.Static_Info
{
    public enum LevelType
    {
        Entrance, Normal, Tutorial, Elite, Boss, SpaceStation
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Difficulty
    {
        public static readonly Difficulty Easy = new("Easy", Color.HSVToRGB(0.36f, 1, 0.75f));
        public static readonly Difficulty Medium = new("Medium", Color.HSVToRGB(0.16f, 1, 0.85f));
        public static readonly Difficulty Difficult = new("Difficult", Color.HSVToRGB(0f, 0.9f, 0.9f));
        public static readonly Difficulty Insane = new("Insane", Color.HSVToRGB(0.83f, 0.7f, 1));
        
        public static readonly Difficulty Entrance = new("Safe", Color.HSVToRGB(0.51f, 0.35f, 0.88f), false);
        public static readonly Difficulty SpaceStation = new("Space Station", Color.HSVToRGB(0f, 0f, 0.7f), false);

        public readonly string Text;
        public readonly Color Color;
        
        public static int Count { get; private set; }
        private Difficulty(string text, Color color, bool count = true)
        {
            Text = text;
            Color = color;

            if (count) Count++;
        }
        
        public static implicit operator Difficulty(int idx) { return (Difficulty) typeof(Difficulty).GetFields(BindingFlags.Static | BindingFlags.Public)[idx].GetValue(null); }
    }
    
    public interface ILevelMetadata {}

    public class ShopMetadata : ILevelMetadata
    {
        public List<IBoostableStat> Stats;

        public static ShopMetadata Generate()
        {
            return new ShopMetadata
            {
                Stats = IBoostableStat.Stats.OrderBy(_ => Random.value).Take(3).ToList()
            };
        }
    }

    public class LevelSpriteData
    {
        public Sprite Sprite, HiddenSprite;
        public float RadiusMult = 1;
    }

    [Serializable]
    public class LoreData
    {
        public string lore;
    }
    
    [Serializable]
    public class LevelLoreData : LoreData 
    {
        public string localName, discovered, voidInfluence;
    }
    
    public class LevelData
    {
        public LevelType Type;
        public int Loot;
        public int MaxTier;
        public int HazardBudget, HazardLoot;
        public int[] Waves;
        public List<int> Connections;
        public LevelSpriteData SpriteData;
        public LoreData LoreData;

        public Vector3 WorldPosition;

        public string Title, Description;
        public bool Travellable;
        public Difficulty Difficulty;

        [CanBeNull] public ILevelMetadata Metadata;
    }

    public class LevelSelectData : MonoBehaviour
    {
        public static LevelSelectData LevelSelectDataInstance => StaticInfoHolder.Instance.GetCachedComponent<LevelSelectData>();
        
        public float baseDifficulty;
        public float hardBaseDifficulty;
        public float gameDifficultyModifier;
        public float hardGameDifficultyModifier;
        public float levelModifier;
        public float hardLevelModifier;
        public float randomModifier;
        public float eliteDifficultyModifier;
        public float hardEliteDifficultyModifier;
        public float galaxyModifier;
        public int[] minBudgetPerWave;

        public bool hardMode;
        
        private float BaseDifficulty => hardMode ? hardBaseDifficulty : baseDifficulty;
        private float GameDifficultyModifier => hardMode ? hardGameDifficultyModifier : gameDifficultyModifier;
        private float LevelModifier => hardMode ? hardLevelModifier : levelModifier;
        private float EliteDifficultyModifier => hardMode ? hardEliteDifficultyModifier : eliteDifficultyModifier;
        
        // based on how difficultyScore is generated below
        public float MaxDifficultyScore => BaseDifficulty + LevelModifier * Levels.Count(l=>l.Type == LevelType.Normal) + randomModifier;
        
        public const int EliteWaves = 3;
        private const int EliteWaveStart = 1; // for difficulty, which wave elites start at (2)
        
        private float EliteDifficulty => EliteDifficultyModifier * minBudgetPerWave[EliteWaveStart..(EliteWaveStart + EliteWaves)].Sum();


#if UNITY_EDITOR
        public void RevealAll()
        {
            var orig = CurrentPlanet;
            for (var i = 0; i < Levels.Length; i++) CurrentPlanet = i;
            CurrentPlanet = orig;
            _visitedPlanets.Clear();
        }
#endif
        [NonSerialized] private int _currentPlanet = -1;
        public int CurrentPlanet
        {
            get => _currentPlanet;
            set
            {
                if (_currentPlanet == value) return;
                _currentPlanet = value;
                if (value < 0) return;

                _visitedPlanets.Add(value);
                foreach (var idx in Levels[value].Connections)
                {
                    var level = Levels[idx];
                    if (level.Waves != null) continue;

                    var difficultyScore = level.Type switch
                    {
                        LevelType.Boss => MaxDifficultyScore,
                        LevelType.Elite => EliteDifficulty,
                        LevelType.SpaceStation => 0,
                        _ => BaseDifficulty + LevelModifier * _visitedPlanets.Count(p=>Levels[p].Type == LevelType.Normal) + Random.Range(0, 1) * randomModifier
                    };
                    
                    var difficultyBudget = (int) (GameDifficultyModifier * difficultyScore + 0/*TODO: galaxyNumber * galaxyModifier*/);
                    List<int> waves = new();

                    // TODO: kinda scuffed?
                    level.HazardBudget = (int) (10 + 30 * difficultyScore/MaxDifficultyScore);
                    level.HazardLoot = (int) (10 + 80 * difficultyScore/MaxDifficultyScore);

                    // start with as many waves as possible given min budget
                    while (true)
                    {
                        var budget = (int)(GameDifficultyModifier *
                                           minBudgetPerWave[waves.Count + (level.Type == LevelType.Elite ? EliteWaveStart : 0)]);
                        if ((difficultyBudget -= budget) < 0)
                        {
                            difficultyBudget += budget;
                            break;
                        }

                        waves.Add(budget);

                        if (level.Type == LevelType.Elite && waves.Count == EliteWaves) break;
                    }
                    // distribute the rest randomly
                    while (difficultyBudget > 0)
                    {
                        var addition = (difficultyBudget < 5 ? difficultyBudget :
                            Random.Range(1, 5));

                        // don't allow waves to go past the next wave's min value
                        var validWaves = minBudgetPerWave.Select((_, i) =>
                        {
                            var wave = i;
                            if (level.Type == LevelType.Elite) i -= EliteWaveStart;
                            if (i < 0 || i >= waves.Count) return -1;

                            return i == waves.Count - 1 || waves[i] + addition < GameDifficultyModifier * minBudgetPerWave[wave + 1] ? i : -1;
                        }).Where(i=>i!=-1).ToList();
                        waves[validWaves[Random.Range(0, validWaves.Count)]] += addition;
                        difficultyBudget -= addition;
                    }

                    if (level.Type != LevelType.Elite)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            if (Random.value < Mathf.Pow(difficultyScore / MaxDifficultyScore, i == 0 ? 1 : 2 * i))
                            {
                                waves.Add(waves[^1]);
                            }
                        }
                    }

                    level.Loot = 16 * Mathf.Clamp((int)(difficultyScore * (Random.value * 0.4 + 0.8)), 0, (int) MaxDifficultyScore);
                    level.MaxTier = (int) (5*Mathf.Pow(difficultyScore/MaxDifficultyScore, 2/3f) + 1);
                    level.Waves = waves.ToArray();

                    var scale = Difficulty.Count/2;
                    level.Difficulty = level.Type switch
                    {
                        LevelType.Entrance => Difficulty.Entrance,
                        LevelType.SpaceStation => Difficulty.SpaceStation,
                        LevelType.Tutorial => Difficulty.Easy,
                        _ => (int)(scale * Mathf.Pow(2, 1.5f * Mathf.Clamp01(difficultyScore / MaxDifficultyScore)) - scale)
                    };
                }
            }
        }

        private readonly List<int> _visitedPlanets = new();
        public ReadOnlyCollection<int> VisitedPlanets => new(_visitedPlanets);

        public LevelData[] Levels { get; private set; }
        public Tuple<int, int>[] Connections { get; private set; }

        public void PopulateData(LevelData[] levels, Tuple<int, int>[] connections)
        {
            Levels = levels;
            Connections = connections;

            _visitedPlanets.Clear();
            CurrentPlanet = 0;
        }
    }
}
