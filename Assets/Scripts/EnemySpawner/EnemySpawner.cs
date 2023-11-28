using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace EnemySpawner
{
    public class EnemySpawner : MonoBehaviour
    {
        public AssetLabelReference variantLabel;
        public List<int> groupBudgetTiers;
        
        public int debugSpawnEnemies; // TODO: debug only

        private readonly Dictionary<string, List<EnemyVariant>> _groups = new();
        
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
                    
                    Addressables.LoadAssetAsync<GameObject>(variant).Completed += dataHandle =>
                    {
                        _groups[group].Add(dataHandle.Result.GetComponent<EnemyVariant>());
                    };
                }
            };
        }

        private void Update()
        {
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
            var budgets = groupBudgetTiers;
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
                var variants = _groups[group];
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