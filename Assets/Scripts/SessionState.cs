using Fusion;
using UnityEngine;

public class SessionState : NetworkBehaviour
{
    public const int MaxPlayers = 8;

    public static SessionState Instance;

    [Networked, Capacity(MaxPlayers)]
    public NetworkArray<PlayerDetails> Players => default;

    public override void Spawned()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool RegisterPlayer(PlayerRef player, string playerName)
    {
        if (!HasStateAuthority)
            return false;

        playerName = CleanPlayerName(playerName, player);
        int emptySlot = -1;

        for (int i = 0; i < Players.Length; i++)
        {
            PlayerDetails details = Players[i];

            if (details.isActive && details.player == player)
            {
                details.playerName = playerName;
                Players.Set(i, details);
                return true;
            }

            if (!details.isActive && emptySlot == -1)
                emptySlot = i;
        }

        if (emptySlot == -1)
            return false;

        Players.Set(emptySlot, new PlayerDetails
        {
            isActive = true,
            player = player,
            characterIndex = -1,
            playerName = playerName,
            score = 0
        });

        return true;
    }

    public bool TrySelectCharacter(PlayerRef player, int characterIndex)
    {
        if (!HasStateAuthority || IsCharacterTaken(characterIndex))
            return false;

        for (int i = 0; i < Players.Length; i++)
        {
            PlayerDetails details = Players[i];

            if (details.isActive && details.player == player)
            {
                if (details.characterIndex >= 0)
                    return false;

                details.characterIndex = characterIndex;
                Players.Set(i, details);
                return true;
            }
        }

        return false;
    }

    public bool TryGetPlayer(PlayerRef player, out PlayerDetails details)
    {
        for (int i = 0; i < Players.Length; i++)
        {
            details = Players[i];

            if (details.isActive && details.player == player)
                return true;
        }

        details = default;
        return false;
    }

    public bool IsCharacterTaken(int characterIndex)
    {
        for (int i = 0; i < Players.Length; i++)
        {
            PlayerDetails details = Players[i];

            if (details.isActive && details.characterIndex == characterIndex)
                return true;
        }

        return false;
    }

    public int SelectedPlayerCount()
    {
        int count = 0;

        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].isActive && Players[i].characterIndex >= 0)
                count++;
        }

        return count;
    }

    public void ReplacePlayer(PlayerRef oldPlayer, PlayerRef newPlayer)
    {
        if (!HasStateAuthority)
            return;

        for (int i = 0; i < Players.Length; i++)
        {
            PlayerDetails details = Players[i];

            if (!details.isActive || details.player != oldPlayer)
                continue;

            details.player = newPlayer;
            Players.Set(i, details);
            return;
        }
    }

    public void AddScore(PlayerRef player, int amount)
    {
        if (!HasStateAuthority)
            return;
        for (int i = 0; i < Players.Length; i++)
        {
            PlayerDetails details = Players[i];

            if (!details.isActive || details.player != player)
                continue;

            details.score += amount;
            Players.Set(i, details);
            Debug.Log($"Adding {amount} score to {details.playerName}, total score - {details.score}, character {details.characterIndex}");
            
            return;
        }
    }

    private static string CleanPlayerName(string playerName, PlayerRef player)
    {
        playerName = playerName?.Trim();

        if (string.IsNullOrEmpty(playerName))
            playerName = $"Rat {player.PlayerId}";

        if (playerName.Length > 16)
            playerName = playerName.Substring(0, 16);

        return playerName;
    }
}
