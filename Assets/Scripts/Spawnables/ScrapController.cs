using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

using static Static_Info.PlayerData;
public class ScrapController : MonoBehaviour
{
    public int value = 1;

    CustomRigidbody2D _crb;
    GameObject _player;

    private Dictionary<int, Sprite[]> _spriteVals;

    [System.Serializable] public class KVpair
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
        rend.sprite = _spriteVals[value][UnityEngine.Random.Range(0, _spriteVals[value].Length)];
        
    }

    void Update()
    {
        var dist = ((Vector2)_player.transform.position - (Vector2)transform.position);
        Vector2 norm = dist.normalized;
        _crb.velocity = norm * _crb.velocity.magnitude;
        _crb.velocity += 30 * norm * Time.deltaTime;

        if(4 * _crb.velocity.sqrMagnitude * Time.deltaTime * Time.deltaTime > dist.sqrMagnitude)
        {
            GameObject.FindGameObjectWithTag("Player").GetComponent<ScrapDisplayController>().Collect(value);
            //PlayerDataInstance.Scrap += value;
            Destroy(gameObject);
        }
    }

}
