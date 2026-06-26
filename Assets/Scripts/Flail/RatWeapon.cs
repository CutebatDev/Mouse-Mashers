using UnityEngine;

public class RatWeapon : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float damageMultiplier = 5f;

    //private void OnCollisionEnter2D(Collision2D other)
    //{
    //    float impactSpeed = other.relativeVelocity.magnitude;
    //    float damage = impactSpeed * damageMultiplier;

    //    EnemyController target = other.collider.GetComponent<EnemyController>();

    //    if (target != null)
    //    {
    //        target.RPC_TakeDamage(damage);
    //    }
    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        float damage = rb.linearVelocity.magnitude * damageMultiplier;

        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy == null)
            return;

        enemy.TakeDamage(damage);
    }
}
