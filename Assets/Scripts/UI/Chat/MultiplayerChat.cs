using Fusion;
using System.Collections.Generic;

public class MultiplayerChat : NetworkBehaviour
{
    public static MultiplayerChat Instance;

    public delegate void MessageReceived(string username, string message);
    public static event MessageReceived OnMessageReceived;

    [Networked, Capacity(100)]
    private NetworkDictionary<PlayerRef, NetworkString<_16>> Usernames { get; }

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
        Usernames.Set(info.Source, username);
    }

    public void SendMessage(string message)
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

        if (Usernames.TryGet(info.Source, out var name))
            username = name.ToString();

        OnMessageReceived?.Invoke(username, message);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestWhisper(string target, string message, RpcInfo info = default)
    {
        foreach (var player in Usernames)
        {
            if (player.Value.ToString() == target)
            {
                string sender = "Unknown";

                if (Usernames.TryGet(info.Source, out var senderName))
                    sender = senderName.ToString();

                RPC_DeliverWhisper(player.Key, sender, message);

                return;
            }
        }

        OnMessageReceived?.Invoke("System", $"User {target} not found");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DeliverWhisper([RpcTarget] PlayerRef target, string sender, string message, RpcInfo info = default)
    {
        OnMessageReceived?.Invoke($"[Whisper] {sender}", message);
    }
}
