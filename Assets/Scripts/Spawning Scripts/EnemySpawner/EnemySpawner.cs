using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LevelSelect;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace EnemySpawner
{
    public class EnemySpawner : MonoBehaviour
    {
        public AssetLabelReference variantLabel;
        public List<int> groupBudgetTiers;

        public GameObject boundaryCircle;
        public LevelSelectData data;
        
        public int debugSpawnEnemies; // TODO: debug only

        private readonly Dictionary<string, List<EnemyVariant>> _groups = new();
        
        private readonly Dictionary<string, bool> _loadedVariants = new();
        
        private void Awake()
        {
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

        private void Update()
        {
            // TODO: debug
            if (Input.GetKeyUp(KeyCode.RightBracket)) _spawnedEnemies.ForEach(Destroy);
            
            if (_groups.Count == 0 || _loadedVariants.ContainsValue(false)) return;
            
            var level = data.Levels[data.CurrentPlanet];

            // TODO: do spawning waves better
            _spawnedEnemies.RemoveAll(g => g == null);
            if (_spawnedEnemies.Count == 0)
            {
                if (++_wave == level.Waves.Length)
                {
                    SceneManager.LoadScene("LevelSelect");
                }
                else
                {
                    foreach (var enemy in GetSpawnedEnemies(level.Waves[_wave]))
                    {
                        var boundarySize = boundaryCircle.transform.localScale / 2;
                        var rad = Random.Range(0, 2 * Mathf.PI);

                        _spawnedEnemies.Add(Instantiate(
                            enemy, 
                            transform.position + 
                            new Vector3(boundarySize.x * Mathf.Cos(rad), boundarySize.y * Mathf.Sin(rad), 0), 
                            Quaternion.identity
                            ));
                    }
                }
            }
            
            if (debugSpawnEnemies != 0)
            {
                foreach (var enemy in GetSpawnedEnemies(debugSpawnEnemies))
                {
                    var rad = Random.Range(0, 2 * Mathf.PI);
                    
                    Instantiate(
                        enemy,
                        transform.position +
                            new Vector3(40*Mathf.Cos(rad), 40*Mathf.Sin(rad), 0),
                        Quaternion.identity
                    );
                }
                
                debugSpawnEnemies = 0;
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