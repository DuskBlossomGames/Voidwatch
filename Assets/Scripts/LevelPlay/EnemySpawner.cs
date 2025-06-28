using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using Menus;
using Player;
using Spawnables;
using Spawnables.Controllers;
using Spawnables.Controllers.Asteroids;
using Spawnables.Controllers.Misslers;
using Spawnables.Damage;
using Static_Info;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Util;
using Random = UnityEngine.Random;
using static Static_Info.LevelSelectData;
using static Static_Info.Statistics;
namespace LevelPlay
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject player;

        public GameObject HUD, fadeIn;
        public PlanetSetup planet;

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
        
        public NewUpgradeManager nUpMan;
        public GameObject debugEnemy;

        private void Awake()
        {
            _isDebug = debugEnemy != null;
            
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
#if UNITY_EDITOR
          if (_isDebug) LevelSelectDataInstance.CurrentPlanet = LevelSelectDataInstance.Levels[0].Connections[0];
#endif
          _level =  LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

          if (_level.Type == LevelType.Elite) fadeIn.SetActive(true);
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
            if (InputManager.GetKeyUp(KeyCode.LeftBracket)) _timeTillExit = 0;
#endif
            
            if (_groups.Count == 0 || _loadedVariants.ContainsValue(false)) return;
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
                    _timeTillExit = 3;
                    _isTerminal = true;
                }
                else if (_level.Type != LevelType.Elite)
                {
                    if (_wave == -1) SpawnWave();
                    else if (!_waitingOnIndicator) StartCoroutine(SpawnWaveWithIndicator());
                }
            }
        }

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
            
            var hazards = GetSpawnedEnemies(_level.HazardBudget, true);
            var lootPer = _level.HazardLoot / hazards.Count;
            
            var sectorSize = 4/3f*Mathf.PI / hazards.Count;
            for (var sector = 0; sector < hazards.Count; sector++)
            {
                var idx = Random.Range(0, hazards.Count);
                var hazard = hazards[idx];
                hazards.RemoveAt(idx);
                    
                var offset = Random.Range(-sectorSize/3, sectorSize/3);

                var theta = 1/3f * Mathf.PI + sectorSize/2 + sector * sectorSize + offset;
                var r = Random.Range(planet.transform.localScale.x / 2 + 30,
                    boundaryCircle.transform.localScale.x / 2 - 20);
                    
                var hazardObj = Instantiate(
                    hazard,
                    transform.position +  
                    new Vector3(r * Mathf.Cos(theta), r * Mathf.Sin(theta), 0),
                    Quaternion.identity);
                
                hazardObj.GetComponent<EnemyVariant>().ScrapPrefab = scrapPrefab;
                hazardObj.GetComponent<EnemyVariant>().ScrapCount = lootPer;
            }

            EditorApplication.isPaused = true;
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
                var r = Random.Range(planet.transform.localScale.x / 2 + 30,
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
            List<GameObject> enemies = GetSpawnedEnemies(_level.Waves[_wave]);
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
                //Debug.LogFormat("Slot {0} has amt {1}", i, pts[i]);
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
                //Debug.LogFormat("Enemy ({0}) has scrap count ({1})", enemies[j].name, enemies[j].GetComponent<EnemyVariant>().ScrapCount);
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

                //Debug.LogFormat("Late Enemy ({0}) has scrap count ({1})", enemy.name, enemy.GetComponent<EnemyVariant>().ScrapCount);
                enemyObj.GetComponent<EnemyVariant>().ScrapPrefab = scrapPrefab;
                enemyObj.GetComponent<EnemyVariant>().ScrapCount = spts[ind++];

                _spawnedEnemies.Add(enemyObj);
            }
        }

        private List<GameObject> GetSpawnedEnemies(int budget, bool hazards = false)
        {
            if (_isDebug) return new List<GameObject> { debugEnemy };
            
            var groupBudgets = new Dictionary<string, int>();

            var enemyGroups = hazards ? _hazardObjects : _groups;
            foreach (var kvp in enemyGroups.ToList())
            {
                kvp.Value.RemoveAll(e => e.tier > _level.MaxTier);
                if (kvp.Value.Count == 0) enemyGroups.Remove(kvp.Key);
            }
            
            
            var groups = enemyGroups.Keys.ToList();
            var budgets = new List<int>(groupBudgetTiers);
            while (groups.Count > 0 && budget > groupBudgetTiers[0])
            {
                budgets.RemoveAll(b => b > budget);

                var group = Random.Range(0, groups.Count);
                budget -= groupBudgets[groups[group]] = budgets[Random.Range(0, budgets.Count)];

                groups.RemoveAt(group);
            }

            // randomly add in the rest
            while (budget > 0)
            {
                var amt = Random.Range(Mathf.Min(budget, 3), Mathf.Min(budget, 10));

                groupBudgets[groupBudgets.Keys.ToList()[Random.Range(0, groupBudgets.Count)]] += amt;
                budget -= amt;
            }


            var enemies = new List<GameObject>();
            foreach (var group in groupBudgets.Keys)
            {
                var groupBudget = groupBudgets[group];
                var variants = new List<EnemyVariant>(enemyGroups[group]);

                while (variants.Count > 0 && groupBudget > 0)
                {
                    var variant = variants[Random.Range(0, variants.Count)];
                    enemies.Add(variant.gameObject);

                    groupBudget -= variant.cost;
                    variants.RemoveAll(v => v.cost > groupBudget);
                }
            }

            return enemies;
        }
    }
}
