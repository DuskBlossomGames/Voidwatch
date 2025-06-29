using System;
using System.Collections.Generic;
using UnityEngine;
using Util;
using Random = UnityEngine.Random;
using static Singletons.Static_Info.PlayerData;
using static Singletons.Static_Info.Statistics;

namespace Spawnables
{
    public class ScrapController : MonoBehaviour
    {
        public int value = 1;

        CustomRigidbody2D _crb;
        GameObject _player;

        private Dictionary<int, Sprite[]> _spriteVals;

        [Serializable] public class KVpair
        {
            public int value;
            public Sprite[] sprites;
        }

        public KVpair[] spriteVals;

        private void Start()
        {
            _spriteVals = new Dictionary<int, Sprite[]>();
            for (int x = 0; x < spriteVals.Length; x++)
            {
                _spriteVals.Add(spriteVals[x].value, spriteVals[x].sprites);
            }

            _crb = GetComponent<CustomRigidbody2D>();
            _player = GameObject.FindGameObjectWithTag("Player");

            var rend = GetComponent<SpriteRenderer>();
            rend.sprite = _spriteVals[value][Random.Range(0, _spriteVals[value].Length)];
        
        }

        void Update()
        {
            var dist = ((Vector2)_player.transform.position - (Vector2)transform.position);
            Vector2 norm = dist.normalized;
            _crb.linearVelocity = norm * _crb.linearVelocity.magnitude;
            _crb.linearVelocity += 30 * norm * Time.deltaTime;

            if(8 * _crb.linearVelocity.sqrMagnitude * Time.deltaTime * Time.deltaTime > dist.sqrMagnitude)
            {
                StatisticsInstance.scrapCollected += value;
                PlayerDataInstance.Scrap += value;
                //PlayerDataInstance.Scrap += value;
                Destroy(gameObject);
            }
        }

    }
}
