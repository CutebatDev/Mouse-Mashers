using System;
using DG.Tweening;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private DamageNumber damagePrefab;
    [SerializeField] private float maxHealth;
    [SerializeField] private float iFrameDuration = 0.25f;
    [SerializeField] private EnemyAI enemyAI;
    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private AudioClip[] hitSounds;
    
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
    public void RPC_TakeDamage(float amount, RpcInfo info = default)
    {
        if (Time.time < lastDamageTime + iFrameDuration)
            return;

        lastDamageTime = Time.time;

        currentHealth -= amount;

        if (currentHealth <= 0 && !isDead)
        {
            currentHealth = 0;
            isDead = true;
            SessionState.Instance.AddScore(info.Source, (int)maxHealth);
            StartDeathSequence();
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 10)
            return;
        ShowDamage(amount, transform.position);
        PlaySquishSound();
        TakeDamageAnimation();

        RPC_TakeDamage(amount);
    }


    private void PlaySquishSound()
    {
        int soundIndex = Random.Range(0, hitSounds.Length);
        Debug.Log($"Playing sound {soundIndex} from {hitSounds.Length}");
        AudioManager.Instance.PlaySfx2D(hitSounds[soundIndex]);
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