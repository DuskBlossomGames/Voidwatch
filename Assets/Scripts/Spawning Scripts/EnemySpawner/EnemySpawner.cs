using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using LevelSelect;
using Spawnables;
using Static_Info;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Util;

using static Static_Info.LevelSelectData;
namespace EnemySpawner
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject player;

        public GameObject HUD, fadeIn;
        public PlanetSetup planet;
        
        public Spawnables.Player.PlayerDamageable playerDamager;
        public GameObject scrapPrefab;
        public List<int> difficultyToLootTiers;
        public List<int> scrapCountTiers;
        public List<float> scrapChanceTiers;

        public AssetLabelReference variantLabel;
        public string miniBossesGroup;
        public List<int> groupBudgetTiers;

        public GameObject boundaryCircle;
        private LevelData _level;

        private readonly Dictionary<string, List<EnemyVariant>> _groups = new();
        private readonly List<GameObject> _miniBosses = new();
        
        private readonly Dictionary<string, bool> _loadedVariants = new();

        public float levelTimer = 0.0f;
        public bool dataWritingOn = true;

        private void Awake()
        {
            _isTerminal = false;
            groupBudgetTiers.Sort();
            
            // dunno if this is the best way to do this, but it works
            Addressables.LoadResourceLocationsAsync(variantLabel).Completed += locHandle =>
            {
                foreach (var variant in locHandle.Result)
                {
                    var group = variant.PrimaryKey.Split("/")[^2];
                    if (!_groups.ContainsKey(group)) _groups[group] = new List<EnemyVariant>();
                    
                    _loadedVariants[group] = false;
                    Addressables.LoadAssetAsync<GameObject>(variant).Completed += dataHandle =>
                    {
                        if (group == miniBossesGroup)
                        {
                            _miniBosses.Add(dataHandle.Result);
                        }
                        else
                        {
                            _groups[group].Add(dataHandle.Result.GetComponent<EnemyVariant>());                            
                        }
                        _loadedVariants[group] = true;
                    };
                }
            };

          _level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];
          print(Mathf.Floor(_level.DifficultyScore/LevelSelectDataInstance.MaxDifficultyScore *5 *2)/2);

          if (_level.Type != LevelType.Elite) fadeIn.SetActive(false);
        }

        private readonly List<GameObject> _spawnedEnemies = new();
        private int _wave = -1;
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
                    _miniBosses[/*TODO: !!!! Random.Range(0, _miniBosses.Count) !!!!*/0],
                    transform.position +
                    new Vector3(-35, 0, 0),
                    Quaternion.identity);
                obj.GetComponent<MiniBoss>().enemySpawner = this;
                _spawnedEnemies.Add(obj);

                _spawnedElite = true;
            }
            
            levelTimer+= Time.deltaTime;
            // TODO: debug
            if (Input.GetKeyUp(KeyCode.RightBracket)) _spawnedEnemies.ForEach(Destroy);
            if (_level.Type == LevelType.Elite && Input.GetKeyUp(KeyCode.Backslash)) for (var i = 1; i < _spawnedEnemies.Count; i++) Destroy(_spawnedEnemies[i]);
            if (Input.GetKeyUp(KeyCode.LeftBracket)) _timeTillExit = 0;

            if (_groups.Count == 0 || _loadedVariants.ContainsValue(false)) return;

            //var level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];

            if (_isTerminal)
            {
                _timeTillExit -= Time.deltaTime;
                if (_timeTillExit < 0)
                {
                    // UpgradePlayer.Upgrade(gun, playerDamager, UpgradePlayer.Upgrades.Overcharge);
                    SceneManager.LoadScene("LevelSelect");
                }
                return;
            }

            // TODO: do spawning waves better
            if (SpawnedEnemies.Count == 0)
            {
                if (_wave == _level.Waves.Length-1)
                {

                    if(dataWritingOn){

                    _level = LevelSelectDataInstance.Levels[LevelSelectDataInstance.CurrentPlanet];
                    var difficultyLocal = Mathf.Floor(_level.DifficultyScore/LevelSelectDataInstance.MaxDifficultyScore *5 *2)/2;
                    var finalTime = levelTimer;
                    string concoconatedDifficultyTimer = "" + difficultyLocal + "," + finalTime;
                    UtilFuncs.WriteToFile("DataScience/LevelClearLevelSelectDataInstance.csv",concoconatedDifficultyTimer);
                    dataWritingOn = false;
                    }

                    _timeTillExit = 3;
                    _isTerminal = true;
                }
                else if (_level.Type != LevelType.Elite)
                {
                    SpawnWave();
                }
            }
        }

        public void SpawnWave()
        {
            if (_wave < _level.Waves.Length - 1) _wave++;
            else return;
            
            //See this link for explanation: https://docs.google.com/presentation/d/1N-m9xBT6kNj14Usj7dzI-zluJXPPxZCACTUyHBboIYs/edit#slide=id.p
            //1.
            List<GameObject> enemies = GetSpawnedEnemies(_level.Waves[_wave]);
            int[] spts = new int[enemies.Count];
            int pointWidth = 0;
            foreach (var enemy in enemies) pointWidth += enemy.GetComponent<EnemyVariant>().cost;

            enemies.Sort(
                delegate (GameObject A, GameObject B)
                    { return A.GetComponent<EnemyVariant>().cost.CompareTo(B.GetComponent<EnemyVariant>().cost); }
                );

            int[] pts = new int[pointWidth];
            float rate = (float)_level.Loot / (float)pointWidth;
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
            for (int i = 0; i < enemies.Count - 1; i++) partitions.Add(Random.Range(1, enemies.Count));


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
                enemies[j].GetComponent<EnemyVariant>().ScrapPrefab = scrapPrefab;
            }

            //Older code
            int ind = 0;
            foreach (var enemy in enemies)
            {
                var boundarySize = boundaryCircle.transform.localScale / 2;
                var rad = Random.Range(0, 2 * Mathf.PI);

                var enemyObj = Instantiate(
                    enemy,
                    transform.position +
                    new Vector3(boundarySize.x * Mathf.Cos(rad), boundarySize.y * Mathf.Sin(rad), 0),
                    Quaternion.identity);
                if (enemyObj.TryGetComponent<MissleShooter>(out var missleShooter)) missleShooter.target = player;

                //Debug.LogFormat("Late Enemy ({0}) has scrap count ({1})", enemy.name, enemy.GetComponent<EnemyVariant>().ScrapCount);
                enemyObj.GetComponent<EnemyVariant>().ScrapCount = spts[ind++];

                _spawnedEnemies.Add(enemyObj);
            }
        }

        private List<GameObject> GetSpawnedEnemies(int budget)
        {
            var groupBudgets = new Dictionary<string, int>();

            var groups = _groups.Keys.ToList();
            var budgets = new List<int>(groupBudgetTiers);
            while (groups.Count > 0 && budget > groupBudgetTiers[0])
            {
                budgets.RemoveAll(b => b > budget);

                var group = Random.Range(0, groups.Count);
                budget -= groupBudgets[groups[group]] = budgets[Random.Range(0, budgets.Count)];

                groups.RemoveAt(group);
            }


            var enemies = new List<GameObject>();
            foreach (var group in groupBudgets.Keys)
            {
                var groupBudget = groupBudgets[group];
                var variants = new List<EnemyVariant>(_groups[group]);

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
