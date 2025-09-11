using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Menus;
using Player;
using Singletons.Static_Info;
using Spawnables;
using Spawnables.Controllers;
using Spawnables.Controllers.Asteroids;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using Random = UnityEngine.Random;
using static Singletons.Static_Info.LevelSelectData;
using static Singletons.Static_Info.Statistics;
namespace LevelPlay
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject player;

        public float fadeInTime;
        public GameObject HUD, fadeIn;

        public WaveIndicatorController wic;

        public GameOverController gameOver;
        
        public PlayerDamageable playerDamager;
        public GameObject scrapPrefab;
        public List<int> difficultyToLootTiers;
        public List<int> scrapCountTiers;
        public List<float> scrapChanceTiers;

        public float minAsteroidSize, maxAsteroidSize;
        
        public AssetLabelReference variantLabel;
        public string miniBossesGroup;
        public List<int> groupBudgetTiers;

        public GameObject boundaryCircle;
        private LevelData _level;

        private bool _spawnedHazards;

        private readonly Dictionary<string, List<EnemyVariant>> _groups = new();
        private readonly Dictionary<string, List<EnemyVariant>> _hazardObjects = new();
        private readonly List<AsteroidSwarmInit> _asteroids = new();
        private readonly List<GameObject> _miniBosses = new();
        
        private readonly Dictionary<string, bool> _loadedVariants = new();
        
        public UpgradeManager nUpMan;
        public GameObject debugEnemy;

        private void Awake()
        {
            _isDebug = LevelSelectDataInstance.Levels == null || debugEnemy != null;
            
            _isTerminal = false;
            groupBudgetTiers.Sort();
            
            // dunno if this is the best way to do this, but it works
            Addressables.LoadResourceLocationsAsync(variantLabel).Completed += locHandle =>
            {
                foreach (var variant in locHandle.Result)
                {
                    var group = variant.PrimaryKey.Split("/")[^2];
                    
                    _loadedVariants[group] = false;
                    Addressables.LoadAssetAsync<GameObject>(variant).Completed += dataHandle =>
                    {
                        _loadedVariants[group] = true;
                        if (group == miniBossesGroup)
                        {
                            _miniBosses.Add(dataHandle.Result);
                        }
                        else
                        {
                            var ev = dataHandle.Result.GetComponent<EnemyVariant>();
                            if (ev == null)
                            {
                                var asteroid = dataHandle.Result.GetComponent<AsteroidSwarmInit>();
                                if (asteroid != null) _asteroids.Add(asteroid);

                                return;
                            }
                            
                            if (ev.hazardObject)
                            {
                                if (!_hazardObjects.ContainsKey(group)) _hazardObjects[group] = new List<EnemyVariant>();

                                _hazardObjects[group].Add(ev);
                            }
                            else
                            {
                                if (!_groups.ContainsKey(group)) _groups[group] = new List<EnemyVariant>();

                                _groups[group].Add(ev);
                            }    
                        }
                    };
                }
            };
    
            // if debug, it's probably being booted from nothing which will make CurrentPlanet 0 TODO: is debug 
            if (_isDebug)
            {
                _level = new LevelData
                {
                    Type = LevelType.Normal,
                    Waves = new [] {0}
                };
            }
            else
            {
                _level =  LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];
            }

            fadeIn.SetActive(true);
            if (_level.Type != LevelType.Elite) StartCoroutine(FadeIn());
            else _faded = true;
        }

        private bool _faded;
        private IEnumerator FadeIn()
        {
            var img = fadeIn.GetComponent<Image>();
            for (float t = 0; t < fadeInTime; t += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();

                img.SetAlpha(1 - t / fadeInTime);
            }

            fadeIn.SetActive(false);

            _faded = true;
        }

        private readonly List<GameObject> _spawnedEnemies = new();
        private int _wave = -1;
        private bool _isDebug;
        private float _timeTillExit;
        private bool _isTerminal;
        
        public List<GameObject> SpawnedEnemies
        {
            get
            {
                _spawnedEnemies.RemoveAll(g => g == null);
                return new List<GameObject>(_spawnedEnemies);
            }
        }

        private bool _spawnedElite;
        private void Update()
        {
            if (_level.Type == LevelType.Elite && !_spawnedElite && _loadedVariants[miniBossesGroup])
            {
                var obj = Instantiate(
                    _miniBosses[Random.Range(0, _miniBosses.Count)],
                    transform.position +
                    new Vector3(-35, 0, 0),
                    Quaternion.identity);
                obj.GetComponent<MiniBoss>().enemySpawner = this;
                _spawnedEnemies.Add(obj);

                _spawnedElite = true;
            }
            
#if UNITY_EDITOR
            if (InputManager.GetKeyUp(KeyCode.RightBracket)) _spawnedEnemies.ForEach(Destroy);
            if (_level.Type == LevelType.Elite && InputManager.GetKeyUp(KeyCode.Backslash)) for (var i = 1; i < _spawnedEnemies.Count; i++) Destroy(_spawnedEnemies[i]);
            if (InputManager.GetKeyUp(KeyCode.LeftBracket))
            {
                _isTerminal = true;
                _timeTillExit = 0;
            }
#endif
            
            if (!_faded || _groups.Count == 0 || _loadedVariants.ContainsValue(false)) return;
            // LevelSelectDataInstance.LogDifficulty(this); // can be uncommented for balancing debugging
            if (!_spawnedHazards && _level.Type != LevelType.Elite) SpawnHazards();

            //var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

            if (_isTerminal)
            {
                _timeTillExit -= Time.deltaTime;
                if (_timeTillExit < 0)
                {
                    // kill all hazards
                    FindObjectsByType<Damageable>(FindObjectsSortMode.None).ToList().ForEach(dmg =>
                    {
                        if ((dmg.gameObject.layer == LayerMask.NameToLayer("Hazards") 
                            || dmg.gameObject.layer == LayerMask.NameToLayer("PlayerOnlyHazard")) 
                            && dmg.GetComponent<AsteroidController>() == null) dmg.Damage(float.MaxValue, null);
                    });

                    StatisticsInstance.levelsCleared++;
                    if (_level.Type == LevelType.Elite)
                    {
                        StartCoroutine(Win());
                        _isTerminal = false;
                    }
                    else
                    {
                        nUpMan.Show();
                        Destroy(gameObject);
                    }
                }
                return;
            }

            if (SpawnedEnemies.Count == 0)
            {
                if (_level.Type == LevelType.Elite || _wave == _level.Waves.Length-1)
                {
                    if (!_isDebug)
                    {
                        _timeTillExit = 3;
                        _isTerminal = true;
                    }
                }
                else if (_level.Type != LevelType.Elite)
                {
                    if (_wave == -1) SpawnWave();
                    else if (!_waitingOnIndicator) StartCoroutine(SpawnWaveWithIndicator());
                }
            }
        }

        public bool WaitingOnIndicator => _waitingOnIndicator;
        private bool _waitingOnIndicator;
        public IEnumerator SpawnWaveWithIndicator()
        {
            if (_waitingOnIndicator) yield break;
            _waitingOnIndicator = true;
            
            yield return wic.Flash();
            SpawnWave();
            
            _waitingOnIndicator = false;
        }

        private bool _won;
        private IEnumerator Win()
        {
            if (_won) yield break;
            _won = true;
            
            player.GetComponent<Movement>().SetInputBlocked(true);
            player.GetComponent<Movement>().autoPilot = true;
            player.GetComponent<PlayerDamageable>().godmode = true;

            yield return gameOver.Run(true, null);
        }

        public void SpawnHazards()
        {
            if (_isDebug) return;
            _spawnedHazards = true;
            
            SpawnAsteroids();
            
            var hazards = GetSpawnedEnemies(_level.HazardBudget, _level.MaxTier, true);
            var lootPer = _level.HazardLoot / hazards.Count;
            
            var sectorSize = 4/3f*Mathf.PI / hazards.Count;
            for (var sector = 0; sector < hazards.Count; sector++)
            {
                var idx = Random.Range(0, hazards.Count);
                var hazard = hazards[idx];
                hazards.RemoveAt(idx);
                    
                var offset = Random.Range(-sectorSize/3, sectorSize/3);

                var theta = 1/3f * Mathf.PI + sectorSize/2 + sector * sectorSize + offset;
                var r = Random.Range(PlanetSetup.Radius + 30,
                    boundaryCircle.transform.localScale.x / 2 - 20);
                    
                var hazardObj = Instantiate(
                    hazard,
                    transform.position +  
                    new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0),
                    Quaternion.identity);
                
                hazardObj.GetComponent<EnemyVariant>().ScrapPrefab = scrapPrefab;
                hazardObj.GetComponent<EnemyVariant>().ScrapCount = lootPer;
            }
        }

        private void SpawnAsteroids()
        {
            
            var asteroidSize = Random.Range(minAsteroidSize, maxAsteroidSize);
            _asteroids.Sort((a,b) => a.size < b.size ? -1 : a.size > b.size ? 1 : 0);

            var asteroids = new List<AsteroidSwarmInit>();
            
            while (asteroidSize > 0)
            {
                var available = _asteroids.Where(a => a.size >= asteroidSize).ToList();
                if (available.Count == 0) available.Add(_asteroids[0]);
                
                var asteroid = available[Random.Range(0, available.Count)];
                var amt = Mathf.Min(asteroidSize, asteroid.size);
                asteroidSize -= amt;

                asteroids.Add(asteroid);
            }
            
            var sectorSize = 7/4f*Mathf.PI / asteroids.Count;
            for (var sector = 0; sector < asteroids.Count; sector++)
            {
                var idx = Random.Range(0, asteroids.Count);
                var asteroid = asteroids[idx];
                    
                var offset = Random.Range(-sectorSize/3, sectorSize/3);
                var theta = 1/8f * Mathf.PI + sectorSize/2 + sector * sectorSize + offset;
                var r = Random.Range(PlanetSetup.Radius + 30,
                    boundaryCircle.transform.localScale.x / 2 - 35);
                    
                var asteroidObj = Instantiate(
                    asteroid.gameObject,
                    transform.position +  
                    new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0),
                    Quaternion.identity);
            }
        }

        public void SpawnWave()
        {
            // if debug, only have one wave (_wave == -1)
            if (_wave < _level.Waves.Length - 1 && (!_isDebug || _wave == -1)) _wave++;
            else return;
            
            //See this link for explanation: https://docs.google.com/presentation/d/1N-m9xBT6kNj14Usj7dzI-zluJXPPxZCACTUyHBboIYs/edit#slide=id.p
            //1.
            List<GameObject> enemies = GetSpawnedEnemies(_level.Waves[_wave], _level.MaxTier);
            int[] spts = new int[enemies.Count];
            int pointWidth = 0;
            foreach (var enemy in enemies) pointWidth += enemy.GetComponent<EnemyVariant>().cost;

            enemies.Sort((a, b) =>
                a.GetComponent<EnemyVariant>().cost.CompareTo(b.GetComponent<EnemyVariant>().cost));

            int[] pts = new int[pointWidth];
            float rate = (float)_level.Loot / (float)pointWidth / _level.Waves.Length;
            float debt = 0;
            for (int i = 0; i < pointWidth; i++)
            {
                debt += rate;
                debt -= pts[i] = Mathf.FloorToInt(debt);
            }

            SortedSet<int> partitions = new SortedSet<int>();
            partitions.Add(0);
            partitions.Add(pointWidth);

            //2.
            for (int i = 0; i < pointWidth - 1; i++) partitions.Add(Random.Range(1, pointWidth));


            for (int i = 0; i < partitions.Count - 1; i++)
            {
                //3.
                int amt = 0; //Sum up all points in point buckets between partitions
                for (int l = partitions.ElementAt(i); l < partitions.ElementAt(i + 1); l++) amt += pts[l];

                //4.
                int k = partitions.ElementAt(i + 1) - 1;
                int j = -1;
                do { k -= enemies[++j].GetComponent<EnemyVariant>().cost; } while (k > 0);
                //Just a fancy way to find which enemy falls on the right side of the partition

                //5.
                spts[j] += amt;
            }
            
            //Older code
            int ind = 0;
            foreach (var enemy in enemies)
            {
                var boundarySize = boundaryCircle.transform.localScale / 2;
                var rad = Random.Range(1/4f * Mathf.PI, 7/4f * Mathf.PI);

                var enemyObj = Instantiate(
                    enemy,
                    transform.position +
                    new Vector3(boundarySize.x * Mathf.Cos(rad), boundarySize.y * Mathf.Sin(rad), 0),
                    Quaternion.Euler(0, 0, 180+Mathf.Rad2Deg * rad));
                if (enemyObj.TryGetComponent<MissleShooter>(out var missleShooter)) missleShooter.target = player;

                enemyObj.GetComponent<EnemyVariant>().ScrapPrefab = scrapPrefab;
                enemyObj.GetComponent<EnemyVariant>().ScrapCount = spts[ind++];

                _spawnedEnemies.Add(enemyObj);
            }
        }

        // TODO: privatize
        public List<GameObject> GetSpawnedEnemies(int budget, int tier, bool hazards = false)
        {
            if (_isDebug) return new List<GameObject> { debugEnemy };
            
            var groupBudgets = new Dictionary<string, int>();

            var enemyGroups = (hazards ? _hazardObjects : _groups)
                .ToDictionary(e=>e.Key, e=>e.Value.ToList());
            foreach (var kvp in enemyGroups.ToList())
            {
                kvp.Value.RemoveAll(e => e.tier > tier);
                if (kvp.Value.Count == 0) enemyGroups.Remove(kvp.Key);
            }

            if (enemyGroups.Count == 0) return new List<GameObject>();
            
            
            var groups = enemyGroups.Keys.ToList();
            var budgets = new List<int>(groupBudgetTiers);
            
            groups.Remove("Platelins"); // only able to spawn platelins after choosing another group
            var hasPlatelins = hazards; // if hazards, we don't want to add back in platelins so just start it as true 
            while (groups.Count > 0 && budget > groupBudgetTiers[0])
            {
                budgets.RemoveAll(b => b > budget);

                var group = Random.Range(0, groups.Count);
                var groupName = groups[group];
                groups.RemoveAt(group);
                
                var minCost = enemyGroups[groupName].Min(v => v.cost);
                var allowedBudgets = budgets.Where(b=>b >=  minCost).ToList();
                if (allowedBudgets.Count == 0) continue;
                
                budget -= groupBudgets[groupName] = allowedBudgets[Random.Range(0, allowedBudgets.Count)];

                if (!hasPlatelins)
                {
                    hasPlatelins = true;
                    groups.Add("Platelins");
                }
            }

            // randomly add in the rest
            while (budget > 0)
            {
                var amt = Random.Range(Mathf.Min(budget, 3), Mathf.Min(budget+1, 10));

                groupBudgets[groupBudgets.Keys.ToList()[Random.Range(0, groupBudgets.Count)]] += amt;
                budget -= amt;
            }

            
            var enemies = new List<GameObject>();

            var leftover = 0;
            foreach (var group in groupBudgets.Keys)
            {
                var groupBudget = groupBudgets[group];
                var variants = enemyGroups[group].Where(v=>v.cost*0.9f <= groupBudget).ToList();

                while (variants.Count > 0 && groupBudget > 0)
                {
                    var variant = variants[Random.Range(0, variants.Count)];
                    enemies.Add(variant.gameObject);

                    groupBudget -= variant.cost;
                    variants.RemoveAll(v => v.cost*0.9f > groupBudget); // have the ability to be close enough
                }

                if (groupBudget > 0) leftover += groupBudget;
            }

            var leftoverVariants = enemyGroups.Values.SelectMany(l=>l.Where(e=>e.cost <= leftover)).ToList();
            while (leftoverVariants.Count > 0 && leftover > 0)
            {
                var variant = leftoverVariants[Random.Range(0, leftoverVariants.Count)];
                leftover -= variant.cost;
                leftoverVariants.RemoveAll(e => e.cost > leftover);
                
                enemies.Add(variant.gameObject);
            }

            return enemies;
        }
    }
}
