using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs")]
    [SerializeField] private NetworkRunner runnerPrefab;
    [SerializeField] private GameObject playerPrefab;

    [Header("References")]
    [SerializeField] private LobbyUI lobbyUI;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private string GAME_SCENE_NAME;

    private readonly List<PlayerRef> players = new();

    private NetState state;
    private NetworkRunner runner;
    private PlayerScript currentPlayer;

    private int readiedPlayers = 0;

    private InputAction devAction;

    [Networked]
    private int ReadiedPlayers
    {
        get
        {
            return readiedPlayers;
        }
        set
        {
            readiedPlayers = value;
        }
    }

    private void Awake()
    {
        devAction = new InputAction(
            name: "DevKey",
            type: InputActionType.Button,
            binding: "<Keyboard>/f1"
            );

        devAction.performed += _ => StartMatch();
    }

    private void OnEnable()
    {
        devAction.Enable();
    }

    private void OnDisable()
    {
        devAction.Disable();
    }

    void Start()
    {
        state = NetState.Disconnected;

        CreateRunner();
    }

    void Update()
    {
            
    }

    private void CreateRunner()
    {
        runner = Instantiate(runnerPrefab);
        runner.AddCallbacks(this);
    }

    private void RefreshRoomUI()
    {
        if (runner.IsRunning && !runner.IsShutdown)
        {
            lobbyUI.UpdatePlayerCount(players.Count);
        }
    }

    public async void JoinLobby()
    {
        // string lobbyName = lobbyUI.LobbyNameText;

        StartGameResult result = await runner.JoinSessionLobby(SessionLobby.Custom,"default");

        if (result.Ok)
        {
            state = NetState.Lobby;
            Debug.Log($"Joined Lobby: default");
        }
        else
        {
            string error = $"Failed to create/join lobby: {result.ErrorMessage}";
            lobbyUI.CreateErrorMessage(error);
            Debug.LogError(error);
            return;
        }

        lobbyUI.UpdateUIState(state);
    }

    public async void CreateRoom(string roomName, int maxPlayers)
    {
        if (string.IsNullOrWhiteSpace(roomName))
            return;

        StartGameResult result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            PlayerCount = maxPlayers,
            OnGameStarted = OnGameStarted
        });

        if (result.Ok)
        {
            state = NetState.InSession;
            Debug.Log($"Created and Joined Room: {roomName}");
        }
        else
        {
            string error = $"Failed to create room: {result.ErrorMessage}";
            lobbyUI.CreateErrorMessage(error);
            Debug.LogError(error);
            return;
        }
    }

    public async void JoinRoom(string roomName)
    {
        StartGameResult result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            OnGameStarted = OnGameStarted
        });

        if (result.Ok)
        {
            state = NetState.InSession;
            Debug.Log($"Joined Room: {roomName}");
        }
        else
        {
            Debug.LogError($"Failed to join room: {result.ErrorMessage}");
            return;
        }
    }

    private void OnGameStarted(NetworkRunner obj)
    {
        lobbyUI.UpdateUIState(state);
        runner.SessionInfo.IsOpen = false;
        runner.SessionInfo.IsVisible = false;
    }

    public void OnCreateRoomPressed()
    {
        string roomName = lobbyUI.RoomNameText;
        int maxPlayers = lobbyUI.RoomMaxPlayers;

        CreateRoom(roomName, maxPlayers);
    }

    public async void OnLeaveSessionPressed()
    {
        if (!runner.IsRunning)
            return;

        await runner.Shutdown();

        await Task.Yield();

        JoinLobby();
    }


    // Beware, callbacks below!

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (state != NetState.InSession)
            return;

        bool isLocalPlayer = runner.LocalPlayer == player;

        Debug.Log($"Player {player.PlayerId} joined, localPlayer: {isLocalPlayer}");

        players.Add(player);

        if (isLocalPlayer)
            currentPlayer = runner.Spawn(playerPrefab, spawnPoints[player.PlayerId - 1].transform.position).GetComponent<PlayerScript>();

        ReadiedPlayers++;

        RefreshRoomUI();

        if (ReadiedPlayers == runner.SessionInfo.MaxPlayers)
            StartMatch();
    }

    //[Rpc]
    //public void TogglePlayerReadyRPC()
    //{
    //    if (!currentPlayer.IsReady)
    //    {
    //        currentPlayer.SetReadyRPC();
    //        ReadiedPlayers++;
    //    }
    //    else
    //    {
    //        currentPlayer.SetUnreadyRPC();
    //        ReadiedPlayers--;
    //    }

    //    Debug.Log($"Current Readied Players: {ReadiedPlayers}");

    //    if (players.Count > 1 && ReadiedPlayers == players.Count)
    //        StartMatch();
    //}

    public void StartMatch()
    {
        if (runner.IsSharedModeMasterClient)
        {
            runner.SessionInfo.IsVisible = false;
            runner.SessionInfo.IsOpen = false;
            
        }
        Debug.Log("Loading Game Scene");
        runner.LoadScene(GAME_SCENE_NAME);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (state != NetState.InSession)
            return;

        players.RemoveAll(p => p == player);

        ReadiedPlayers--;

        RefreshRoomUI();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (state != NetState.Lobby)
            return;

        lobbyUI.UpdateSessions(sessionList);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Shutdown: {shutdownReason}");

        players.Clear();

        CreateRunner();

        lobbyUI.UpdateUIState(state);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}