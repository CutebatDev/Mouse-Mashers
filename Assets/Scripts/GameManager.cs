using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    private Dictionary<string, PlayerRef> userIdPlayersMap = new Dictionary<string, PlayerRef>();
    
    public static GameManager Instance;
    public Camera mainCamera;
    public GameObject playerPrefab;

    public NetworkRunner networkRunner;

    public string lobbySceneName;
    [SerializeField] private bool shutdownRunnerOnEndGame = true;

    [SerializeField] public InputAction quitAction;
    private bool isReturningToMenu;
    private bool menuLoadStarted;
    private bool returnToMenuStarted;
    private bool callbacksRegistered;

    [SerializeField] private AudioClip mainGameplayMusic;
    
    
    private void Awake()
    {
        Instance = this;
        networkRunner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        if (quitAction == null)
            return;

        quitAction.performed += OnQuitPerformed;
        quitAction.Enable();
    }

    private void OnDisable()
    {
        if (quitAction == null)
            return;

        quitAction.performed -= OnQuitPerformed;
        quitAction.Disable();
    }

    void Start()
    {
        networkRunner = GetRunner();
        RegisterRunnerCallbacks();

        if (quitAction == null)
            Debug.LogWarning("GameManager has no quit action assigned.");
        else if (quitAction.bindings.Count == 0)
            Debug.LogWarning("GameManager quit action has no input binding assigned.");
        
        AudioManager.Instance.PlayMusic(mainGameplayMusic);
    }

    private void OnQuitPerformed(InputAction.CallbackContext context)
    {
        AudioManager.Instance.StopMusic();
        RequestEndGame();
    }

    public void RequestEndGame()
    {
        RPC_RequestEndGame();
    }

    private NetworkRunner GetRunner()
    {
        if (networkRunner != null && networkRunner.IsRunning)
            return networkRunner;

        if (Runner != null)
        {
            networkRunner = Runner;
            return networkRunner;
        }

        networkRunner = NetworkRunner.GetRunnerForScene(SceneManager.GetActiveScene());
        return networkRunner;
    }
    
    public override void Spawned()
    {
        base.Spawned();
        networkRunner = Runner;
        RegisterRunnerCallbacks();
        RPCRequestSpawn(SelectedCharacter.Index);
    }

    private void RegisterRunnerCallbacks()
    {
        if (callbacksRegistered)
            return;

        NetworkRunner runner = GetRunner();
        if (runner == null)
            return;

        runner.AddCallbacks(this);
        callbacksRegistered = true;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCRequestSpawn(int character, RpcInfo info = default) 
    {
        NetworkRunner runner = GetRunner();
        string userId = runner.GetPlayerUserId(info.Source);
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
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestEndGame(RpcInfo info = default)
    {
        LoadEndGameScene();
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_EndGame(RpcInfo info = default)
    {
        LoadEndGameScene();
    }

    private void LoadEndGameScene()
    {
        if (string.IsNullOrWhiteSpace(lobbySceneName))
        {
            Debug.LogError("Cannot end game: lobbySceneName is empty.");
            return;
        }

        NetworkRunner runner = GetRunner();
        if (runner == null)
        {
            Debug.LogError("Cannot end game: no active NetworkRunner found.");
            return;
        }

        if (shutdownRunnerOnEndGame)
        {
            SendEveryoneToMenu();
            return;
        }

        runner.LoadScene(lobbySceneName);
    }

    private void SendEveryoneToMenu()
    {
        if (returnToMenuStarted)
            return;

        returnToMenuStarted = true;

        ReturnToMenu();
    }

    private async void ReturnToMenu()
    {
        if (isReturningToMenu)
            return;

        isReturningToMenu = true;

        AudioManager.Instance.StopMusic();
        NetworkRunner runner = GetRunner();
        if (runner != null && runner.IsRunning)
            await runner.Shutdown();

        LoadMenuSceneOnce();
    }

    private void LoadMenuSceneOnce()
    {
        if (menuLoadStarted)
            return;

        menuLoadStarted = true;
        SceneManager.LoadScene(lobbySceneName);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCSpawnPlayer([RpcTarget] PlayerRef targetPlayer, int character)
    {
        NetworkObject spawnedPlayer = GetRunner().Spawn(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity,
            targetPlayer
        );

        SetFlailCharacter flailCharacter = spawnedPlayer.GetComponent<SetFlailCharacter>();

        flailCharacter.Character = character;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPCRequestAllAuthorityBack([RpcTarget] PlayerRef targetPlayer, PlayerRef oldPlayer)
    {
        List<NetworkObject> networkObjects = GetRunner().GetAllNetworkObjects();
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
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (shutdownRunnerOnEndGame)
            ReturnToMenu();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        if (isReturningToMenu || shutdownRunnerOnEndGame)
            LoadMenuSceneOnce();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        if (isReturningToMenu || shutdownRunnerOnEndGame)
            ReturnToMenu();
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
