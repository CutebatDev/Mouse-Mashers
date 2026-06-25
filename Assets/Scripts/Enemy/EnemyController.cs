using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    private float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;    
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        EnemyRegistry.Instance.UnRegister(this);
    }
}
