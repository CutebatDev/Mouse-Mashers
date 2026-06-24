using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public const string LOBBY_SCENE_NAME = "LobbyScene";
    
    private Dictionary<string, PlayerRef> userIdPlayersMap = new Dictionary<string, PlayerRef>();
    
    public static GameManager Instance;
    public Camera mainCamera;
    public GameObject playerPrefab;

    private NetworkRunner networkRunner;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        networkRunner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
    }

    public override void Spawned()
    {
        base.Spawned();
        RPCRequestSpawn(SelectedCharacter.Index);
    }

    public void LeaveGame()
    {
        if (networkRunner.IsRunning)
        {
            networkRunner.Shutdown();
        }

        SceneManager.LoadScene(LOBBY_SCENE_NAME);
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCRequestSpawn(int character, RpcInfo info = default) 
    {
        string userId = networkRunner.GetPlayerUserId(info.Source);
        if(userIdPlayersMap.TryGetValue(userId, out PlayerRef playerRef))
        {
           Debug.Log("It's a rejoin!");
           RPCRequestAllAuthorityBack(info.Source, playerRef);
           userIdPlayersMap[userId] = info.Source;
        }
        else
        {
            userIdPlayersMap[userId] = info.Source;
            RPCSpawnPlayer(info.Source, character);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCSpawnPlayer([RpcTarget] PlayerRef targetPlayer, int character)
    {
        NetworkObject spawnedPlayer = networkRunner.Spawn(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity,
            targetPlayer
        );

        SetFlailCharacter flailCharacter = spawnedPlayer.GetComponent<SetFlailCharacter>();

        flailCharacter.Character = character;
        flailCharacter.SetCharacter(character);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCRequestAllAuthorityBack([RpcTarget] PlayerRef targetPlayer, PlayerRef oldPlayer)
    {
        List<NetworkObject> networkObjects = networkRunner.GetAllNetworkObjects();
        networkObjects = networkObjects.Where(o => o.StateAuthority == oldPlayer).ToList();
        foreach (var networkObject in networkObjects)
        {
            networkObject.RequestStateAuthority();
        }
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // if (player == runner.LocalPlayer)
        // {
        //     networkRunner.SpawnAsync(playerPrefab, Vector3.zero, Quaternion.identity, player);
        // }
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

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
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
}
