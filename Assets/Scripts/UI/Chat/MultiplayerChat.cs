using Fusion;
using System.Collections.Generic;

public class MultiplayerChat : NetworkBehaviour
{
    public static MultiplayerChat Instance;

    public delegate void MessageReceived(string username, string message);
    public static event MessageReceived OnMessageReceived;

    private Dictionary<PlayerRef, string> usernames = new();

    public override void Spawned()
    {
        Instance = this;
    }

    public void SetUsername(string username)
    {
        RPC_SetUsername(username);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetUsername(string username, RpcInfo info = default)
    {
        usernames[info.Source] = username;
    }

    public void SendMessage(string username, string message)
    {
        RPC_SendMessage(message);
    }

    public void SendWhisper(string target, string message)
    {
        RPC_RequestWhisper(target, message);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        string username = "Unknown";

        if (usernames.TryGetValue(info.Source, out string name))
            username = name;

        OnMessageReceived?.Invoke(username, message);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestWhisper(string target, string message, RpcInfo info = default)
    {
        foreach (var player in usernames)
        {
            if (player.Value == target)
            {
                RPC_DeliverWhisper(player.Key, usernames[info.Source], message);
            }

            return;
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DeliverWhisper([RpcTarget] PlayerRef target, string sender, string message, RpcInfo info = default)
    {
        OnMessageReceived?.Invoke($"[Whisper] {sender}", message);
    }
}
