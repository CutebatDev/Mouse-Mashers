using System.Collections.Generic;
using UnityEngine;

public class EnemyRegistry : MonoBehaviour
{
    private List<EnemyController> registeredEnemies;

    public List<EnemyController> RegisteredEnemies => registeredEnemies;

    public static EnemyRegistry Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        registeredEnemies = new List<EnemyController>();
    }

    public void Register(EnemyController enemy)
    {
        registeredEnemies.Add(enemy);
    }

    public void UnRegister(EnemyController enemy)
    {
        registeredEnemies.Remove(enemy);
    }
}
