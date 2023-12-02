using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpawnEnemies : MonoBehaviour
{
    public int debugspawnpts;
    public bool spawn;

    public EnemySpawner.EnemySpawner enemySpawner;
    // Start is called before the first frame update
    void Start()
    {
        if (debugspawnpts != 0)
        {
            StartCoroutine(DelayedSpawn());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawn)
        {
            enemySpawner.debugSpawnEnemies = debugspawnpts;
            debugspawnpts = 0;
            spawn = false;
        }
    }
    IEnumerator DelayedSpawn()
    {
        yield return new WaitForSeconds(.1f);
        enemySpawner.debugSpawnEnemies = debugspawnpts;
        debugspawnpts = 0;
    }
}
