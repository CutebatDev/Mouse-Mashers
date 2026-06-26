using Fusion;
using JetBrains.Annotations;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform worldCanvas;
    [SerializeField] private Transform enemyRoot;
    [SerializeField] private EnemyController[] enemyPrefabs;
    //[SerializeField] private Transform[] spawnPoints;

    [Header("Spawning")]
    [SerializeField] private int maxEnemyCount = 10;
    [SerializeField] private int spawnThreshold = 3;
    [SerializeField] private int minToSpawn = 2;
    [SerializeField] private int maxToSpawn = 5;
    [SerializeField] private float spawnDelay = 3f;
    [SerializeField] private int maxSpawnAttempts = 20;
    [SerializeField] private float minSpawnDistance = 2f;

    private Bounds bounds;

    private float spawnTimer;
    [CanBeNull] private NetworkRunner networkRunner;

    void Start()
    {
        bounds = FloorManager.Floor.bounds;
    }

    private NetworkRunner GetNetworkRunner()
    {
        if (!networkRunner)
        {
            networkRunner = GameManager.Instance.networkRunner;
        }
        return networkRunner;
        
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        if(!GetNetworkRunner().IsSharedModeMasterClient)
            return;
        spawnTimer -= Time.fixedDeltaTime;
        
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
        if(!GetNetworkRunner().IsSharedModeMasterClient)
            return;

        for (int i = 0;  i < amount; i++)
        {
            NetworkObject temp = GetNetworkRunner()
                .Spawn(ChooseEnemy().gameObject, GetSpawnPosition(), Quaternion.identity);

            EnemyController enemy = temp.GetComponent<EnemyController>();

            EnemyRegistry.Instance.Register(enemy);
            enemy.SetWorldCanvasRef(worldCanvas);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 candidate = new Vector3(
                
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                bounds.center.z
            );

            bool tooClose = false;

            foreach (EnemyController enemy in EnemyRegistry.Instance.RegisteredEnemies)
            {
                if(Vector2.Distance(candidate, enemy.transform.position) < minSpawnDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
                return candidate;
        }

        return bounds.center;
    }

    //private Transform ChooseSpawnPoint()
    //{
    //    int spawnIndex = Random.Range(0, spawnPoints.Length);
    //    return spawnPoints[spawnIndex];
    //}

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