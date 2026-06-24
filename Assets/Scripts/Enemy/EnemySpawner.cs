using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private EnemyController[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawning")]
    [SerializeField] private int maxEnemyCount = 10;
    [SerializeField] private int spawnThreshold = 3;
    [SerializeField] private int minToSpawn = 2;
    [SerializeField] private int maxToSpawn = 5;
    [SerializeField] private float spawnDelay = 3f;

    private float spawnTimer;

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            TrySpawnEnemies();
            spawnTimer = spawnDelay;
        }
    }

    private void TrySpawnEnemies()
    {
        int currentCount = EnemyRegistry.Instance.RegisteredEnemies.Count;

        if (currentCount > spawnThreshold)
            return;

        int amount = ChooseSpawnAmount();

        int spaceLeft = maxEnemyCount - currentCount;
        amount = Mathf.Min(amount, spaceLeft);

        SpawnEnemies(amount);
    }

    private void SpawnEnemies(int amount)
    {
        for (int i = 0;  i < amount - 1; i++)
        {
            EnemyController enemy = Instantiate(ChooseEnemy(), ChooseSpawnPoint().position, Quaternion.identity, enemyRoot);
            EnemyRegistry.Instance.Register(enemy);
        }
    }

    private Transform ChooseSpawnPoint()
    {
        int spawnIndex = Random.Range(0, spawnPoints.Length);
        return spawnPoints[spawnIndex];
    }

    private EnemyController ChooseEnemy()
    {
        int prefabIndex = Random.Range(0, enemyPrefabs.Length);
        return enemyPrefabs[prefabIndex];
    }

    private int ChooseSpawnAmount()
    {
        return Random.Range(minToSpawn, maxToSpawn + 1);
    }
}
