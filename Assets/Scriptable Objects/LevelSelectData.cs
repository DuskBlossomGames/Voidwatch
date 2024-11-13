using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static readonly LevelType SpaceStation = new("A brief respite for the weary. Who might you find?");

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
        public int Loot;
        public int DifficultyScore;
        public int[] Waves;
        public Sprite Sprite;
        public List<int> Connections;
        public Sprite HiddenSprite;

        public Vector3 WorldPosition;

        public string Name;
        public string LoreText;
    }

    public class LevelSelectData : ScriptableObject
    {
        public float baseDifficulty;
        public float gameDifficultyModifier;
        public float levelModifier;
        public float randomModifier;
        public float galaxyModifier;
        public int[] minBudgetPerWave;

        // based on how difficultyScore is generated below
        public float MaxDifficultyScore => baseDifficulty +
                                           levelModifier * (Levels.Length - 1) + 2 * randomModifier;


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
                        baseDifficulty + levelModifier * (_visitedPlanets.Count - 1) + Random.Range(0, 2) * randomModifier;
                    var difficultyBudget = (int) (gameDifficultyModifier * (difficultyScore + 0/*TODO: galaxyNumber * galaxyModifier*/));
                    List<int> waves = new();

                    // start with as many waves as possible given min budget
                    while (true)
                    {
                        var budget = minBudgetPerWave[waves.Count];
                        if ((difficultyBudget -= budget) < 0)
                        {
                            difficultyBudget += budget;
                            break;
                        }

                        waves.Add(budget);
                    }
                    // distribute the rest randomly
                    while (difficultyBudget > 0)
                    {
                        var addition = difficultyBudget < 5 ? difficultyBudget :
                            Random.Range(0, difficultyBudget);

                        waves[Random.Range(0, waves.Count - 1)] += addition;
                        difficultyBudget -= addition;
                    }

                    level.Loot = Mathf.Clamp((int)(difficultyScore * (Random.value * 0.4 + 0.8)), 0, (int) MaxDifficultyScore);
                    level.DifficultyScore = (int) difficultyScore;
                    level.Waves = waves.ToArray();
                }
            }
        }

        [NonSerialized] private readonly List<int> _visitedPlanets = new();
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

        public void ClearData()
        {
            _currentPlanet = -1;
            Levels = null;
            Connections = null;
        }
    }
}
