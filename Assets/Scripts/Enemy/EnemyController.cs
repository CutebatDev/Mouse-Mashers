using DG.Tweening;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private DamageNumber damagePrefab;
    [SerializeField] private float maxHealth;
    [SerializeField] private float iFrameDuration = 0.25f;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private SpriteRenderer sr;

    private bool isTakingDamage = false;
    private bool isDead = false;

    [Networked] private float currentHealth { get; set; }

    private float lastDamageTime;
    private RectTransform worldCanvas;

    public override void Spawned()
    {
        base.Spawned();
        currentHealth = maxHealth;
    }

    public void SetWorldCanvasRef(RectTransform canvas)
    {
        worldCanvas = canvas;
    }

    private void ShowDamage(float amount, Vector3 position)
    {
        DamageNumber number = Instantiate(damagePrefab, position, Quaternion.identity, worldCanvas);
        number.transform.SetPositionAndRotation(position, Quaternion.identity);
        number.SetDamage(amount);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TakeDamage(float amount)
    {
        if (Time.time < lastDamageTime + iFrameDuration)
            return;

        lastDamageTime = Time.time;

        currentHealth -= amount;

        if (currentHealth <= 0 && !isDead)
        {
            currentHealth = 0;
            isDead = true;

            StartDeathSequence();
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 10)
            return;
        ShowDamage(amount, transform.position);
        TakeDamageAnimation();

        RPC_TakeDamage(amount);
    }
    
    private void TakeDamageAnimation()
    {
        if (isTakingDamage || isDead)
            return;

        isTakingDamage = true;

        Color originalColor = sr.color;

        sr.DOKill();
        sr.color = Color.red;

        DOVirtual.DelayedCall(iFrameDuration, () =>
        {
            if (sr == null) return;

            sr.color = originalColor;
            isTakingDamage = false;
        }).SetLink(gameObject);
    }


    private void StartDeathSequence()
    {
        sr.DOKill();

        RPC_PlayDeathVFX();

        DOVirtual.DelayedCall(iFrameDuration, () =>
        {
            Die();
        }).SetLink(gameObject);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayDeathVFX()
    {
        sr.color = Color.red;
    }

    private void Die()
    {
        EnemyRegistry.Instance.UnRegister(this);
        GameManager.Instance.networkRunner.Despawn(Object);
    }
}