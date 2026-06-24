using System;
using Fusion;
using UnityEngine;
public class SetFlailCharacter : NetworkBehaviour
{
    [SerializeField] private Sprite[] ratSprites;
    [SerializeField] private SpriteRenderer ratSpriteRenderer;
    [Networked] public int Character
    {
        get
        {
            return Character;
        }
        set
        {
            Character = value;
            RPC_UpdateCharacter();
        }
    }

    [Rpc]
    private void RPC_UpdateCharacter()
    {
        SetCharacter(Character);
    }
    
    public void SetCharacter(int character)
    {
        ratSpriteRenderer.sprite = ratSprites[character];
    }
}