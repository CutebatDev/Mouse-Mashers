using Fusion;

public struct PlayerDetails : INetworkStruct
{
    public NetworkBool isActive;
    public PlayerRef player;
    public int characterIndex;
    public NetworkString<_16> playerName;
    public int score;
}
