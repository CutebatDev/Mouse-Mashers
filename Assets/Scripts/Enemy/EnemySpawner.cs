using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private EnemyController[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int minSpawnThreshold;
    //[SerializeField] private int maxSpawnThreshold;
    [SerializeField] private int minToSpawn;
    [SerializeField] private int maxToSpawn;

    void Update()
    {
        SpawnerLoop();
    }

    private Transform ChooseSpawnPoint()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[spawnIndex];
    }

    private void SpawnEnemy(int amount)
    {
        for (int i = 0;  i < spawnPoints.Length - 1; i++)
        {
            EnemyController enemy = Instantiate(ChooseEnemy(), ChooseSpawnPoint().position, Quaternion.identity, enemyRoot);
            EnemyRegistry.Instance.Register(enemy);
        }
    }

    private EnemyController ChooseEnemy()
    {
        int prefabIndex = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[prefabIndex];
    }

    private int ChooseSpawnAmount()
    {
        return Random.Range(minToSpawn, maxToSpawn);
    }

    private void SpawnerLoop()
    {
        // Add spawn delay (Coroutine)

        if (EnemyRegistry.Instance.RegisteredEnemies.Count > minSpawnThreshold)
            return;

        SpawnEnemy(ChooseSpawnAmount());
    }
}
