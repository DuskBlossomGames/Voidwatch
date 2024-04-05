using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace EnemySpawner
{
    public class EnemyVariant : MonoBehaviour
    {
        public int cost;
        public int ScrapCount;
        public GameObject ScrapPrefab;

        GameObject _spawntar;
        bool _spawned;

        private void Start()
        {
            _spawned = false;
            _spawntar = FindDmgable(gameObject);
            _spawntar.GetComponent<Spawnables.EnemyDamageable>().varientParent = gameObject;
        }

        public void SpawnScrap(Vector3 pos)
        {
            if (!_spawned) { 
                _spawned = true;
                for (var i = 0; i < ScrapCount; i++)
                {
                    var rigid = Instantiate(ScrapPrefab, pos, transform.rotation).GetComponent<CustomRigidbody2D>();
                    rigid.velocity = Random.insideUnitCircle * 5;
                }
            }
            
        }

        GameObject FindDmgable(GameObject tar)
        {
            if (tar.GetComponent<Spawnables.EnemyDamageable>() != null)
            {
                return tar;
            }
            else
            {
                for (int i = 0; i < tar.transform.childCount; i++)
                {
                    GameObject ret = FindDmgable(tar.transform.GetChild(i).gameObject);
                    if (ret != null)
                    {
                        return ret;
                    }
                }
                return null;
            }
        }
    }
}