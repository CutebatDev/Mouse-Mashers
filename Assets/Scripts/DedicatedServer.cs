using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class DedicatedServer : NetworkBehaviour, INetworkRunnerCallbacks
{
    public const string GameModeSessionProperty = "game_mode";
    private INetworkRunnerCallbacks _networkRunnerCallbacksImplementation;
    [SerializeField] private NetworkRunner networkRunner;
    [SerializeField] private SessionState sessionStatePrefab;
    [SerializeField] private string GAME_SCENE_NAME;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;


    private NetState state;
        
    private readonly List<PlayerRef> players = new();
    
    
    private int readiedPlayers = 0;
    private bool matchStarted;
    
    private void Start()
    {
        state = NetState.Disconnected;
        networkRunner.AddCallbacks(this);
        CreateRoom("Dedicated Server Room", 6);
    }
    
    private async void CreateRoom(string roomName, int maxPlayers)
    {
        if (string.IsNullOrWhiteSpace(roomName))
            return;

        StartGameResult result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Server,
            CustomLobbyName = "default",
            SessionName = roomName,
            PlayerCount = maxPlayers,
            IsOpen = true,
            SessionProperties = new Dictionary<string, SessionProperty>
            {
                { GameModeSessionProperty, maxPlayers },
                { "mode", (int)GameMode.Server }
            }
        });
        
        if (result.Ok)
        {
            state = NetState.InSession;
            Debug.Log($"Created and Joined Room: {roomName}");
        }
        else
        {
            string error = $"Failed to create room: {result.ErrorMessage}";
            Debug.LogError(error);
        }
    }
    private void Update()
    {
        TryStartMatchIfEveryoneReady();
    }
    private void TryStartMatchIfEveryoneReady()
    {
        if (matchStarted || state != NetState.InSession)
            return;

        if (!networkRunner || !networkRunner.IsRunning || !networkRunner.IsServer)
            return;

        if (players.Count == 0)
            return;

        readiedPlayers = CountReadyPlayers();

        if (readiedPlayers == players.Count)
            StartMatch();
    }

    private int CountReadyPlayers()
    {
        int readyCount = 0;

        if (networkRunner == null || !networkRunner.IsRunning)
            return readyCount;

        foreach (NetworkObject networkObject in networkRunner.GetAllNetworkObjects())
        {
            if (!networkObject.IsValid)
                continue;

            PlayerScript player = networkObject.GetComponent<PlayerScript>();

            if (player != null && player.IsReady)
                readyCount++;
        }

        return readyCount;
    }
    
    public void StartMatch()
    {
        if (matchStarted)
            return;

        if (!networkRunner || !networkRunner.IsRunning)
            return;

        if (!networkRunner.IsServer)
            return;

        if (!sessionStatePrefab)
        {
            Debug.LogError("Cannot start match: SessionState prefab is not assigned.");
            return;
        }

        matchStarted = true;

        SessionState sessionState = networkRunner.Spawn(
            sessionStatePrefab,
            flags: NetworkSpawnFlags.DontDestroyOnLoad
        );

        foreach (NetworkObject networkObject in networkRunner.GetAllNetworkObjects())
        {
            if (!networkObject.IsValid)
                continue;

            PlayerScript lobbyPlayer = networkObject.GetComponent<PlayerScript>();

            if (lobbyPlayer != null)
            {
                sessionState.RegisterPlayer(
                    networkObject.StateAuthority,
                    lobbyPlayer.PlayerName.ToString()
                );
            }
        }

        if (networkRunner.IsServer)
        {
            networkRunner.SessionInfo.IsVisible = false;
            networkRunner.SessionInfo.IsOpen = false;
        }

        Debug.Log("Loading Game Scene");
        networkRunner.LoadScene(GAME_SCENE_NAME);
    }
    
    
    
    #region callbacks

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (state != NetState.InSession)
            return;
        
        Debug.Log($"Player {player.PlayerId} joined");

        if (!players.Contains(player))
            players.Add(player);
        
        int spawnIndex = Mathf.Abs(player.AsIndex) % spawnPoints.Length;

        NetworkObject playerObject = runner.Spawn(
            playerPrefab,
            spawnPoints[spawnIndex].position,
            Quaternion.identity,
            inputAuthority: player
        );

        runner.SetPlayerObject(player, playerObject);

        PlayerScript playerScript = playerObject.GetComponent<PlayerScript>();
        playerScript.SetPlayerName($"Rat number {player.AsIndex}");
        
        readiedPlayers = CountReadyPlayers();
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

    #endregion
}