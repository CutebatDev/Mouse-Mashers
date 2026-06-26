using Fusion;
using UnityEngine;

public class RatWeapon : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float damageMultiplier = 5f;

    [SerializeField] private AudioClip[] squickySounds;

    private NetworkObject ownerObject;

    private void Awake()
    {
        ownerObject = GetComponentInParent<NetworkObject>();
    }

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
        if (ownerObject == null || !ownerObject.HasStateAuthority)
            return;

        float damage = rb.linearVelocity.magnitude * damageMultiplier;

        EnemyController enemy = other.GetComponent<EnemyController>();

        if (enemy == null)
            return;

        PlaySquickySound();
        enemy.TakeDamage(damage);
    }


    private void PlaySquickySound()
    {
        AudioManager.Instance.PlaySfx2D(
            squickySounds[Random.Range(0,squickySounds.Length)]
            );
    }
}
