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
    [SerializeField] private string postGameSceneName = "Post Game Screen";
    [SerializeField] private float matchDuration = 60f;
    [SerializeField] private bool shutdownRunnerOnEndGame = true;

    [Networked] private TickTimer MatchTimer { get; set; }
    [Networked] private NetworkBool MatchEndRequested { get; set; }

    [SerializeField] public InputAction quitAction;
    private bool isReturningToMenu;
    private bool menuLoadStarted;
    private bool returnToMenuStarted;
    private NetworkRunner callbackRunner;
    private bool postGameLoadStarted;
    private bool localEndRequested;

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

    private void OnDestroy()
    {
        if (callbackRunner != null)
            callbackRunner.RemoveCallbacks(this);

        callbackRunner = null;

        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        networkRunner = GetRunner();
        RegisterRunnerCallbacks(GetRunner());

        if (quitAction == null)
            Debug.LogWarning("GameManager has no quit action assigned.");
        else if (quitAction.bindings.Count == 0)
            Debug.LogWarning("GameManager quit action has no input binding assigned.");
        
        if (Runner.GameMode != GameMode.Server)
            AudioManager.Instance.PlayMusic(mainGameplayMusic);
    }

    private void OnQuitPerformed(InputAction.CallbackContext context)
    {
        if (localEndRequested)
            return;

        localEndRequested = true;
        quitAction.Disable();
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
        RegisterRunnerCallbacks(Runner);

        if (Object.HasStateAuthority)
        {
            MatchEndRequested = false;
            MatchTimer = TickTimer.CreateFromSeconds(Runner, matchDuration);
        }

        if (Runner.LocalPlayer.IsRealPlayer)
            RPCRequestSpawn();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority || postGameLoadStarted)
            return;

        if (MatchEndRequested || MatchTimer.Expired(Runner))
            LoadPostGameScene();
    }

    private void RegisterRunnerCallbacks(NetworkRunner runner)
    {
        if (runner == null || callbackRunner == runner)
            return;

        if (callbackRunner != null)
            callbackRunner.RemoveCallbacks(this);

        runner.AddCallbacks(this);
        callbackRunner = runner;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPCRequestSpawn(RpcInfo info = default) 
    {
        NetworkRunner runner = GetRunner();
        string userId = runner.GetPlayerUserId(info.Source);
        if(userIdPlayersMap.TryGetValue(userId, out PlayerRef playerRef))
        {
           Debug.Log("It's a rejoin!");
           RPCRequestAllAuthorityBack(info.Source, playerRef);
           SessionState.Instance?.ReplacePlayer(playerRef, info.Source);
           userIdPlayersMap[userId] = info.Source;
        }
        else
        {
            if (SessionState.Instance == null ||
                !SessionState.Instance.TryGetPlayer(info.Source, out PlayerDetails details))
            {
                Debug.LogError($"Cannot spawn {info.Source}: no PlayerDetails found.");
                return;
            }

            userIdPlayersMap[userId] = info.Source;
            if (runner.GameMode == GameMode.Server)
                ServerSpawnPlayer(info.Source, details.characterIndex);
            else
                RPCSpawnPlayer(info.Source, details.characterIndex);
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
    public void RPC_RequestEndGame(RpcInfo info = default)
    {
        MatchEndRequested = true;
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority | RpcTargets.InputAuthority)]
    public void RPC_EndGame(RpcInfo info = default)
    {
        MatchEndRequested = true;
    }

    private void LoadPostGameScene()
    {
        if (postGameLoadStarted)
            return;

        if (string.IsNullOrWhiteSpace(postGameSceneName))
        {
            Debug.LogError("Cannot end game: postGameSceneName is empty.");
            return;
        }

        NetworkRunner runner = GetRunner();
        if (runner == null)
        {
            Debug.LogError("Cannot end game: no active NetworkRunner found.");
            return;
        }

        MatchEndRequested = true;
        MatchTimer = TickTimer.None;
        postGameLoadStarted = true;
        Debug.Log("Match ended. Loading Post Game Screen once.");
        runner.LoadScene(postGameSceneName);
    }

    private void SendEveryoneToMenu()
    {
        if (returnToMenuStarted)
            return;

        returnToMenuStarted = true;

        ReturnToMenu();
    }

    public void ActivateReturnToMenu()
    {
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
    
    [Rpc(RpcSources.StateAuthority | RpcSources.InputAuthority, RpcTargets.All)]
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
    
    private void ServerSpawnPlayer(PlayerRef player, int characterIndex)
    {
        NetworkObject playerObject = Runner.Spawn(
            playerPrefab,
            Vector3.zero,
            Quaternion.identity,
            inputAuthority: player
        );

        Runner.SetPlayerObject(player, playerObject);

        SetFlailCharacter character =
            playerObject.GetComponent<SetFlailCharacter>();

        character.Character = characterIndex;
    }

    [Rpc(RpcSources.StateAuthority | RpcSources.InputAuthority, RpcTargets.All)]
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
        if (Mouse.current == null)
            return;

        Camera cameraToUse = mainCamera != null ? mainCamera : Camera.main;
        if (cameraToUse == null)
            return;

        Vector2 screenPosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = cameraToUse.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                -cameraToUse.transform.position.z
            )
        );

        input.Set(new PlayerInputData
        {
            IsPressed = Mouse.current.leftButton.isPressed,
            WorldPosition = worldPosition
        });
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
