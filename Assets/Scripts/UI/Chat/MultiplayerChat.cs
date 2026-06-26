using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerChat : NetworkBehaviour
{
    public static MultiplayerChat Instance;

    public delegate void MessageReceived(string username, string message);
    public static event MessageReceived OnMessageReceived;

    public override void Spawned()
    {
        Instance = this;
    }

    //public void SetUsername()
    //{
    //    username = usernameInput.text;
    //}

    public void SendMessage(string username, string message)
    {
        RPC_SendMessage(username, message);
    }

    //public void CallMessageRPC()
    //{
    //    string message = input.text;
    //    RPC_SendMessage(username, message);
    //}

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SendMessage(string username, string message, RpcInfo rpcInfo = default)
    {
        OnMessageReceived?.Invoke(username, message);

        //_messages.text += $"{username}: {message}\n";
    }
}
