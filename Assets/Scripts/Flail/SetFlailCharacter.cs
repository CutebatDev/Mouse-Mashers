using Fusion;
using UnityEngine;

public class SetFlailCharacter : NetworkBehaviour
{
    
    [Networked, OnChangedRender(nameof(OnCharacterChanged))]
    public int Character { get; set; }
    
    [SerializeField] private Sprite[] ratSprites;
    [SerializeField] private SpriteRenderer ratSpriteRenderer;
    [SerializeField] private Transform handle;
    [SerializeField] private float directionDeadZone = 0.05f;

    public override void Spawned()
    {
        base.Spawned();
        SetCharacter(Character);
    }

    private void OnCharacterChanged()
    {
        SetCharacter(Character);
    }

    private void LateUpdate()
    {
        Vector2 awayFromHandle = ratSpriteRenderer.transform.position - handle.position;

        if (awayFromHandle.sqrMagnitude <= directionDeadZone * directionDeadZone)
            return;

        float angle = Mathf.Atan2(awayFromHandle.y, awayFromHandle.x) * Mathf.Rad2Deg + 90f;
        ratSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetCharacter(int character)
    {
        if (character < 0 || character >= ratSprites.Length)
        {
            Debug.LogError($"Bad character index: {character}");
            return;
        }

        ratSpriteRenderer.sprite = ratSprites[character];
    }
}
