using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Static_Info
{
    public class LevelType
    {
        public static readonly LevelType Entrance = new("The entrance to the galaxy. Where you just came from!");
        public static readonly LevelType Normal = new("The Cult of the Void has control. Can you break them?");
        public static readonly LevelType Elite = new("The Cult of the Void has control. Can you bre~kskxzsh");
        public static readonly LevelType Boss = new("The Void beckons...");
        public static readonly LevelType SpaceStation = new("A brief respite for the weary. Who might you find?");

        public readonly string Description;

        private LevelType() { }
        private LevelType(string description)
        {
            Description = description;
        }

        // TODO: warp (wormhole)
        // TODO: other NPCs

    }

    public class LevelData
    {
        public LevelType Type;
        public int Loot;
        public int DifficultyScore, MaxTier;
        public int HazardBudget, HazardLoot;
        public int[] Waves;
        public Sprite Sprite;
        public List<int> Connections;
        public Sprite HiddenSprite;

        public Vector3 WorldPosition;

        public string Name;
        public string LoreText;

        public bool IsBoss;
    }

    public class LevelSelectData : MonoBehaviour
    {
        public static LevelSelectData LevelSelectDataInstance => StaticInfoHolder.Instance.GetCachedComponent<LevelSelectData>();

        public float baseDifficulty;
        public float gameDifficultyModifier;
        public float levelModifier;
        public float randomModifier;
        public float galaxyModifier;
        public int[] minBudgetPerWave;

        // based on how difficultyScore is generated below
        public float MaxDifficultyScore => baseDifficulty + levelModifier * (Levels.Length - 1) + randomModifier;
        
        public const int EliteWaves = 3;
        private const int EliteWaveStart = 1; // for difficulty, which wave elites start at (2)
        
        private float EliteDifficulty => 1.1f * minBudgetPerWave[EliteWaveStart..(EliteWaveStart + EliteWaves)].Sum();


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
                _currentPlanet = value;
                if (value < 0) return;

                _visitedPlanets.Add(value);
                foreach (var idx in Levels[value].Connections)
                {
                    var level = Levels[idx];
                    if (level.Waves != null) continue;

                    var difficultyScore = level.Type == LevelType.Boss ? MaxDifficultyScore :
                        level.Type == LevelType.Elite ? EliteDifficulty :
                        baseDifficulty + levelModifier * (_visitedPlanets.Count - 1) + Random.Range(0, 1) * randomModifier;
                    
                    var difficultyBudget = (int) (gameDifficultyModifier * difficultyScore + 0/*TODO: galaxyNumber * galaxyModifier*/);
                    List<int> waves = new();

                    // TODO: kinda scuffed?
                    level.HazardBudget = (int) (10 + 30 * difficultyScore/MaxDifficultyScore);
                    level.HazardLoot = (int) (10 + 80 * difficultyScore/MaxDifficultyScore);

                    // start with as many waves as possible given min budget
                    while (true)
                    {
                        var budget = (int)(gameDifficultyModifier *
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

                            return i == waves.Count - 1 || waves[i] + addition < gameDifficultyModifier * minBudgetPerWave[wave + 1] ? i : -1;
                        }).Where(i=>i!=-1).ToList();
                        waves[validWaves[Random.Range(0, validWaves.Count)]] += addition;
                        difficultyBudget -= addition;
                    }

                    for (var i = 0; i < 3; i++)
                    {
                        if (Random.value < Mathf.Pow(difficultyScore / MaxDifficultyScore, i == 0 ? 1 : 2 * i))
                        {
                            waves.Add(waves[^1]);
                        }
                    }

                    level.Loot = 16 * Mathf.Clamp((int)(difficultyScore * (Random.value * 0.4 + 0.8)), 0, (int) MaxDifficultyScore);
                    level.DifficultyScore = (int) difficultyScore;
                    level.MaxTier = (int) (5*Mathf.Pow(difficultyScore/MaxDifficultyScore, 2/3f) + 1);
                    level.Waves = waves.ToArray();
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
