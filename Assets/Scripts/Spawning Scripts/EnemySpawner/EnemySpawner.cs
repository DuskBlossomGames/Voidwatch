using System.Collections.Generic;
using System.Linq;
using LevelSelect;
using Spawnables;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace EnemySpawner
{
    public class EnemySpawner : MonoBehaviour
    {
        public GameObject scrapPrefab;
        public List<int> difficultyToLootTiers;
        public List<int> scrapCountTiers;
        public List<float> scrapChanceTiers;
        
        public AssetLabelReference variantLabel;
        public List<int> groupBudgetTiers;

        public GameObject boundaryCircle;
        public LevelSelectData data;
        
        private readonly Dictionary<string, List<EnemyVariant>> _groups = new();
        
        private readonly Dictionary<string, bool> _loadedVariants = new();
        
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
                        _groups[group].Add(dataHandle.Result.GetComponent<EnemyVariant>());
                        _loadedVariants[group] = true;
                    };
                }
            };
        }

        private readonly List<GameObject> _spawnedEnemies = new();
        private int _wave = -1;
        private float _timeTillExit;
        private bool _isTerminal;

        private void Update()
        {
            // TODO: debug
            if (Input.GetKeyUp(KeyCode.RightBracket)) _spawnedEnemies.ForEach(Destroy);
            if (Input.GetKeyUp(KeyCode.LeftBracket)) _timeTillExit = 0;

            if (_groups.Count == 0 || _loadedVariants.ContainsValue(false)) return;

            var level = data.Levels[data.CurrentPlanet];

            if (_isTerminal)
            {
                _timeTillExit -= Time.deltaTime;
                if(_timeTillExit<0) SceneManager.LoadScene("LevelSelect");
                return;
            }

            // TODO: do spawning waves better
            _spawnedEnemies.RemoveAll(g => g == null);
            if (_spawnedEnemies.Count == 0)
            {
                if (++_wave == level.Waves.Length)
                {
                    _timeTillExit = 3;
                    _isTerminal = true;
                }
                else
                {
                    //See this link for explanation: https://docs.google.com/presentation/d/1N-m9xBT6kNj14Usj7dzI-zluJXPPxZCACTUyHBboIYs/edit#slide=id.p
                    
                    //1.
                    List<GameObject> enemies = GetSpawnedEnemies(level.Waves[_wave]);
                    int pointWidth = 0;
                    foreach (var enemy in enemies) pointWidth += enemy.GetComponent<EnemyVariant>().cost;

                    enemies.Sort(
                        delegate (GameObject A, GameObject B)
                            { return A.GetComponent<EnemyVariant>().cost.CompareTo(B.GetComponent<EnemyVariant>().cost); }
                        );

                    int[] pts = new int[pointWidth];
                    float rate = (float)level.Loot / (float)pointWidth;
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
                        enemies[j].GetComponent<EnemyVariant>().ScrapCount += amt;
                        //Debug.LogFormat("Enemy ({0}) has scrap count ({1})", enemies[j].name, enemies[j].GetComponent<EnemyVariant>().ScrapCount);
                        enemies[j].GetComponent<EnemyVariant>().ScrapPrefab = scrapPrefab;
                    }

                    //Older code
                    foreach (var enemy in enemies)
                    {
                        var boundarySize = boundaryCircle.transform.localScale / 2;
                        var rad = Random.Range(0, 2 * Mathf.PI);

                        var enemyObj = Instantiate(
                            enemy,
                            transform.position +
                            new Vector3(boundarySize.x * Mathf.Cos(rad), boundarySize.y * Mathf.Sin(rad), 0),
                            Quaternion.identity);

                        //Debug.LogFormat("Late Enemy ({0}) has scrap count ({1})", enemy.name, enemy.GetComponent<EnemyVariant>().ScrapCount);

                        _spawnedEnemies.Add(enemyObj);
                    }

                }
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