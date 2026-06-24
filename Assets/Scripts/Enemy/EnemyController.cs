using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;

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
