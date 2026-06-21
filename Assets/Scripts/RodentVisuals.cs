using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class RodentVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [FormerlySerializedAs("duration")] [SerializeField] private float flashDuration = 0.2f;
    
    MaterialPropertyBlock _materialPropertyBlock;
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private bool _isPlayingHitEffect = false;

    private void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
        PlayHitEffect();
    }

    public void PlayHitEffect()
    {
        if (_isPlayingHitEffect)
            return;
        
        StartCoroutine(HitEffectAnimation());
    }

    private IEnumerator HitEffectAnimation()
    {
        _isPlayingHitEffect = true;
        
        // Set initial values
        spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(FlashAmountId, 0f);
        spriteRenderer.SetPropertyBlock(_materialPropertyBlock);

        // Setup Sequence
        Sequence hitSequence = DOTween.Sequence();

        // Tween to full flash amount
        Tween flashIn = DOVirtual.Float(0f, 1f, flashDuration / 2f, value =>
        {
            spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(FlashAmountId, value);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        }).SetEase(Ease.OutQuad);

        // Tween back to 0
        Tween flashOut = DOVirtual.Float(1f, 0f, flashDuration / 2f, value =>
        {
            spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(FlashAmountId, value);
            spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
        }).SetEase(Ease.InQuad);
        
        hitSequence.Append(flashIn).Append(flashOut);
        
        yield return hitSequence.WaitForCompletion();
        
        _isPlayingHitEffect = false;
    } 
}
