using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Events;

public class StartInSession : MonoBehaviour, INetworkRunnerCallbacks
{
        [SerializeField] private NetworkRunner runnerPrefab;
        private NetworkRunner networkRunner;
        
        [SerializeField] private UnityEvent<NetworkRunner, PlayerRef> onPlayerJoined;

    private void Start()
    {
        networkRunner = Instantiate(runnerPrefab);
        networkRunner.AddCallbacks(this);
        JoinLobby();
    }

    public async void JoinLobby()
    {
        StartGameResult result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, "DEV");

        if (result.Ok)
        {
            Debug.Log($"Joined DEV");
            JoinRoom();
        }
        else
        {
            Debug.LogError($"Failed to create/join lobby: {result.ErrorMessage}");
        }
    }
    public async void JoinRoom()
    {
        StartGameResult result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "DEV",
            PlayerCount = 8,
            OnGameStarted = OnGameStarted
        });

        if (result.Ok)
        {
            Debug.Log($"Joined Session: DEV");
        }
        else
        {
            Debug.LogError($"Failed to join session: {result.ErrorMessage}");
        }
    }

    public void OnGameStarted(NetworkRunner runner)
    {
        Debug.Log($"YES GAME STARTED WTH?? IS IN SESSION - {runner.IsInSession}");
    }

    #region Callbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player Joined the session");
        onPlayerJoined.Invoke(runner, player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
        byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
        ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

        #endregion
}