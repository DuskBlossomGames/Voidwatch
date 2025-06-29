using System;
using System.Linq;
using Spawnables.Damage;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;

namespace Spawnables.Controllers
{
    public enum EnemyType
    {
        None,
        Mechanical,
        Organic,
        Worm,
        Carcadon,
        WormBoss
    }

    public class EnemyVariant : MonoBehaviour
    {
        public bool hazardObject;
        public int cost;
        
        [Range(1, 5)] public int tier = 1;
        public EnemyType enemyType; // TODO: give values for these
        
        [NonSerialized] public int ScrapCount;
        [NonSerialized] public GameObject ScrapPrefab;

        private bool _spawnedScrap;

        private void Awake()
        {
            foreach (var dmg in GetComponentsInChildren<EnemyDamageable>()) dmg.Variant = this;
        }

        public void SpawnScrap(Vector3 pos)
        {
            if (ScrapPrefab == null || _spawnedScrap) return;
            _spawnedScrap = true;

            var scrapVals = ScrapPrefab.GetComponent<ScrapController>().spriteVals.Select(s => s.value).ToArray();
            while(true)
            {
                var i = Random.Range(0, scrapVals.Length);
                while(scrapVals[i] > ScrapCount) if (i-- == 0) return;

                ScrapCount -= scrapVals[i];
                var rigid = Instantiate(ScrapPrefab, pos, transform.rotation).GetComponent<CustomRigidbody2D>();
                rigid.gameObject.GetComponent<ScrapController>().value = scrapVals[i];
                rigid.linearVelocity = Random.insideUnitCircle * 5;
            }

        }
    }
}