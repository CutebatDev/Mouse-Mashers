using Fusion;
using UnityEngine;

public class SetFlailCharacter : NetworkBehaviour
{
    [SerializeField] private Sprite[] ratSprites;
    [SerializeField] private SpriteRenderer ratSpriteRenderer;

    [Networked, OnChangedRender(nameof(OnCharacterChanged))]
    public int Character { get; set; }

    public override void Spawned()
    {
        base.Spawned();
        SetCharacter(Character);
    }

    private void OnCharacterChanged()
    {
        SetCharacter(Character);
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