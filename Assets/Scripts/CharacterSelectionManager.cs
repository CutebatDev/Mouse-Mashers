using Fusion;
using UnityEngine;

public class CharacterSelectionManager : NetworkBehaviour
{
    [SerializeField] private CharacterButtonVisuals[] buttons;
    [SerializeField] private string gameplaySceneName = "MultiplayerFlail";

    private bool localPickPending;
    private bool localPickConfirmed;

    public override void Spawned()
    {
        if (SessionState.Instance == null)
            return;

        for (int i = 0; i < buttons.Length; i++)
            buttons[i].SetTaken(SessionState.Instance.IsCharacterTaken(i));
    }

    public void RequestPick(int characterIndex)
    {
        if (localPickPending || localPickConfirmed)
            return;

        if (characterIndex < 0 || characterIndex >= buttons.Length)
            return;

        localPickPending = true;
        SetLocalButtonsInteractable(false);
        RPC_RequestPick(characterIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPick(int characterIndex, RpcInfo info = default)
    {
        if (characterIndex < 0 || characterIndex >= buttons.Length)
        {
            RPC_RejectPick(info.Source);
            return;
        }

        SessionState sessionState = SessionState.Instance;

        if (sessionState == null)
        {
            Debug.LogError("Cannot pick character: SessionState is missing.");
            RPC_RejectPick(info.Source);
            return;
        }

        if (!sessionState.TrySelectCharacter(info.Source, characterIndex))
        {
            RPC_RejectPick(info.Source);
            return;
        }

        RPC_ConfirmPick(info.Source, characterIndex);

        if (sessionState.SelectedPlayerCount() >= Runner.SessionInfo.PlayerCount)
        {
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;

            Runner.LoadScene(gameplaySceneName);
            AudioManager.Instance.StopMusic();
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
