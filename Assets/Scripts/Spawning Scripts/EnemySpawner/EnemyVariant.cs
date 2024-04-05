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
        private int[] _scrapvals;

        GameObject _spawntar;
        bool _spawned;

        private void Start()
        {
            var spv = ScrapPrefab.GetComponent<ScrapController>().spriteVals;
            _scrapvals = new int[spv.Length];
            for (int i = 0; i < spv.Length; i++)
            {
                _scrapvals[i] = spv[i].value;
            }
            _spawned = false;
            _spawntar = FindDmgable(gameObject);
            _spawntar.GetComponent<Spawnables.EnemyDamageable>().varientParent = gameObject;
        }

        public void SpawnScrap(Vector3 pos)
        {
            if (!_spawned) { 
                _spawned = true;
                bool run = true;
                while(run)
                {
                    int ind = Random.Range(0, _scrapvals.Length);
                    while(_scrapvals[ind] > ScrapCount)
                    {
                        if (ind == 0) { run = false; break; }
                        ind -= 1;
                    }

                    if (run)
                    {
                        ScrapCount -= _scrapvals[ind];
                        var rigid = Instantiate(ScrapPrefab, pos, transform.rotation).GetComponent<CustomRigidbody2D>();
                        rigid.gameObject.GetComponent<ScrapController>().value = _scrapvals[ind];
                        rigid.velocity = Random.insideUnitCircle * 5;
                    }
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