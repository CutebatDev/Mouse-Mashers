using DG.Tweening;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private DamageNumber damagePrefab;
    [SerializeField] private float maxHealth;
    [SerializeField] private float iFrameDuration = 0.25f;
    [SerializeField] private SpriteRenderer sr;

    private bool isTakingDamage = false;
    private bool isDead = false;

    [Networked] private float currentHealth { get; set; }

    private float lastDamageTime;

    private RectTransform worldCanvas;

    [CanBeNull] private NetworkRunner networkRunner;
    
    
    public override void Spawned()
    {
        base.Spawned();
        currentHealth = maxHealth;    }

    private NetworkRunner GetNetworkRunner()
    {
        if (!networkRunner)
        {
            networkRunner = GameManager.Instance.networkRunner;
        }
        return networkRunner;
        
    }
    
    public void SetWorldCanvasRef(RectTransform canvas)
    {
        worldCanvas = canvas;
    }

    private void ShowDamage(float amount, Vector3 position)
    {
        DamageNumber number = Instantiate(damagePrefab, position, Quaternion.identity, worldCanvas);
        number.transform.SetParent(worldCanvas, false);
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
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount > 0)
        {
            ShowDamage(amount, transform.position);
            TakeDamageAnimation();
        }
        
        RPC_TakeDamage(amount);
        
    }

    private void TakeDamageAnimation()
    {
        if (isTakingDamage)
            return;

        isTakingDamage = true;

        var originalColor = sr.color;
        
        sr.DOKill();
        
        sr.color = Color.red;
        
        DOVirtual.DelayedCall(iFrameDuration, () =>
        {
            sr.color = originalColor;
            isTakingDamage = false;
            if(isDead)
                Die();
        });
    }
    
    private void Die()
    {
        EnemyRegistry.Instance.UnRegister(this);
        GameManager.Instance.networkRunner.Despawn(Object);
    }
}

