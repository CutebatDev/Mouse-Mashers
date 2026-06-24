using Fusion;
using UnityEngine;

public class CharacterSelectionManager : NetworkBehaviour
{
    [SerializeField] private CharacterButtonVisuals[] buttons;
    [SerializeField] private string gameplaySceneName = "MultiplayerFlail";

    private bool[] takenCharacters;
    private int pickedPlayers;

    private void Awake()
    {
        takenCharacters = new bool[buttons.Length];
    }

    public void RequestPick(int characterIndex)
    {
        SelectedCharacter.Index = characterIndex;
        RPC_RequestPick(characterIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestPick(int characterIndex, RpcInfo info = default)
    {
        Debug.Log($"Requesting Pick, character ID {characterIndex}");
        if (characterIndex < 0 || characterIndex >= takenCharacters.Length)
            return;

        if (takenCharacters[characterIndex])
            return;

        takenCharacters[characterIndex] = true;
        pickedPlayers++;

        RPC_ConfirmPick(info.Source, characterIndex);

        if (pickedPlayers >= Runner.SessionInfo.PlayerCount)
        {
            Runner.SessionInfo.IsVisible = false;
            Runner.SessionInfo.IsOpen = false;

            Runner.LoadScene(gameplaySceneName);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ConfirmPick(PlayerRef player, int characterIndex)
    {
        Debug.Log($"Confirming Pick, character ID {characterIndex}");
        buttons[characterIndex].SetTaken(true);

        if (player == Runner.LocalPlayer)
        {
            SelectedCharacter.Index = characterIndex;
        }
    }
}