using Fusion;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private DamageNumber damagePrefab;
    [SerializeField] private float maxHealth;
    [SerializeField] private float iFrameDuration = 0.25f;

    [Networked] private float currentHealth { get; set; }

    private float lastDamageTime;

    private RectTransform worldCanvas;

    public override void Spawned()
    {
        base.Spawned();
        currentHealth = maxHealth;    }

    public void SetWorldCanvasRef(RectTransform canvas)
    {
        worldCanvas = canvas;
    }

    private void ShowDamage(float amount, Vector3 position)
    {
        DamageNumber number = Instantiate(damagePrefab, position, Quaternion.identity, worldCanvas);

        number.SetDamage(amount);
        number.transform.position = position;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float amount)
    {
        if (Time.time < lastDamageTime + iFrameDuration)
            return;

        lastDamageTime = Time.time;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        if (amount > 0)
            ShowDamage(amount, transform.position);
    }

    private void Die()
    {
        EnemyRegistry.Instance.UnRegister(this);
        GameManager.Instance.networkRunner.Despawn(Object);
    }
}

