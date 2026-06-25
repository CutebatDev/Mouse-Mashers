using Fusion;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private float maxHealth;

    [Networked] private float currentHealth { get; set; }

    public override void Spawned()
    {
        base.Spawned();
        currentHealth = maxHealth;    
        
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"IM TRYING TO TAKE DAMAGE HERE, TOOK {amount} AND NOW IM AT {currentHealth}");
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        EnemyRegistry.Instance.UnRegister(this);
        GameManager.Instance.networkRunner.Despawn(Object);
    }
}