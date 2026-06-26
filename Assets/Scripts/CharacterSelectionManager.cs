using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class CharacterSelectionManager : NetworkBehaviour
{
    [SerializeField] private CharacterButtonVisuals[] buttons;
    [SerializeField] private string gameplaySceneName = "MultiplayerFlail";

    private bool[] takenCharacters;
    private readonly Dictionary<PlayerRef, int> playerCharacters = new();
    private bool localPickPending;
    private bool localPickConfirmed;

    private void Awake()
    {
        takenCharacters = new bool[buttons.Length];
    }

    public void RequestPick(int characterIndex)
    {
        if (localPickPending || localPickConfirmed)
            return;

        if (characterIndex < 0 || characterIndex >= buttons.Length)
            return;

        localPickPending = true;
        SelectedCharacter.Index = characterIndex;
        SetLocalButtonsInteractable(false);
        RPC_RequestPick(characterIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPick(int characterIndex, RpcInfo info = default)
    {
        if (characterIndex < 0 || characterIndex >= takenCharacters.Length)
        {
            RPC_RejectPick(info.Source);
            return;
        }

        if (takenCharacters[characterIndex])
        {
            RPC_RejectPick(info.Source);
            return;
        }

        if (playerCharacters.ContainsKey(info.Source))
        {
            RPC_RejectPick(info.Source);
            return;
        }

        takenCharacters[characterIndex] = true;
        playerCharacters[info.Source] = characterIndex;

        RPC_ConfirmPick(info.Source, characterIndex);

        if (playerCharacters.Count >= Runner.SessionInfo.PlayerCount)
        {
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;

            AudioManager.Instance.StopMusic();
            Runner.LoadScene(gameplaySceneName);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ConfirmPick(PlayerRef player, int characterIndex)
    {
        buttons[characterIndex].SetTaken(true);

        if (player == Runner.LocalPlayer)
        {
            localPickPending = false;
            localPickConfirmed = true;
            SelectedCharacter.Index = characterIndex;
            SetLocalButtonsInteractable(false);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_RejectPick([RpcTarget] PlayerRef targetPlayer)
    {
        localPickPending = false;
        SetLocalButtonsInteractable(true);
    }

    private void SetLocalButtonsInteractable(bool interactable)
    {
        foreach (CharacterButtonVisuals button in buttons)
            button.SetInteractable(interactable);
    }
}
