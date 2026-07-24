using System;
using Fusion;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LobbyManager lobbyManager;

    [SerializeField] private TextMeshProUGUI numberOfPlayersInSessionText;

    [SerializeField] private GameObject baseObject;
    [SerializeField] private Button endSessionButton;
    [SerializeField] private Button JoinLobbyButton;
    [SerializeField] private Button CreateRoomButton;

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject sessionPanel;
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private GameObject SessionLoadingBlockingPanel;
    [SerializeField] private Button returnToLobbyBtn;

    [SerializeField] private TMP_Dropdown maxPlayersDropdown;
    [SerializeField] private TMP_Dropdown filterMaxPlayersDropdown;

    [SerializeField] private TextMeshProUGUI lobbyNameTextInput;
    [SerializeField] private TextMeshProUGUI roomNameTextInput;

    
    [SerializeField] private GameObject ErrorPrefab;
    
    public Toggle isPublicToggle;
    
    public string LobbyNameText => lobbyNameTextInput.text;
    public string RoomNameText => roomNameTextInput.text;
    public int RoomMaxPlayers => int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);
    public int FilteredRoomMaxPlayers => int.Parse(filterMaxPlayersDropdown.options[filterMaxPlayersDropdown.value].text);

    [SerializeField] private Button sessionButton;

    private Dictionary<string, Button> sessionButtons = new();

    private List<SessionInfo> savedSessions;

    private void Start()
    {
        JoinLobbyOnClick();
    }

    public void UpdatePlayerCount(int playerCount)
    {
        numberOfPlayersInSessionText.text = $"Players: {playerCount}";
    }

    public void UpdateUIState(NetState state)
    {
        SessionLoadingBlockingPanel.SetActive(false);
        playerPanel.SetActive(state == NetState.Lobby);
        menuPanel.SetActive(state == NetState.Disconnected);
        lobbyPanel.SetActive(state == NetState.Lobby);
        sessionPanel.SetActive(state == NetState.InSession);
        bottomPanel.SetActive(state == NetState.Lobby);
        endSessionButton.interactable = (state == NetState.InSession);
        CreateRoomButton.interactable = (state == NetState.Lobby);
        returnToLobbyBtn.gameObject.SetActive(state == NetState.Lobby);
    }

    public void JoinLobbyOnClick()
    {
        lobbyManager.JoinLobby();
        JoinLobbyButton.interactable = false;
    }
    public void CreateSessionOnClick()
    {
        lobbyManager.OnCreateRoomPressed();
        CreateRoomButton.interactable = false;
        SessionLoadingBlockingPanel.SetActive(true);
    }

    public void CreateSessionUI(string roomName, int maxPlayers, int currentPlayers, bool isFull, bool isOpen, GameMode sessionType)
    {
        Button btn = Instantiate(sessionButton, lobbyPanel.transform);

        btn.onClick.AddListener(() => lobbyManager.JoinRoom(roomName));

        btn.interactable = !isFull || !isOpen;

        TMP_Text text = btn.GetComponentInChildren<TMP_Text>();

        if (sessionType == GameMode.Server)
        {
            currentPlayers--;
            maxPlayers--;
        }            

        text.text = $"{roomName} {currentPlayers}/{maxPlayers}";

        sessionButtons[roomName] = btn;
    }

    public void UpdateSessionPlayerCount(string roomName, int currentPlayerCount, GameMode sessionType)
    {
        Debug.Log("UpdateSessionPlayerCount");

        if (!sessionButtons.TryGetValue(roomName, out Button btn))
            return;

        TMP_Text text = btn.GetComponentInChildren<TMP_Text>();

        string[] parts = text.text.Split(' ');
        if (parts.Length < 2)
            return;

        string countPart = parts[1];
        string[] counts = countPart.Split('/');

        if (counts.Length != 2)
            return;

        string maxPlayers = counts[1];

        if (sessionType == GameMode.Server)
        {
            currentPlayerCount--;
            //maxPlayers--;
        }

        text.text = $"{roomName} {currentPlayerCount}/{maxPlayers}";
    }

    public void UpdateSessionsDrop()
    {
        UpdateSessions(null);
    }

    public void UpdateSessions([CanBeNull] List<SessionInfo> sessions)
    {
        if(sessions != null)
            savedSessions = sessions;
        HashSet<string> activeSessions = new();
        int selectedGameMode = FilteredRoomMaxPlayers;

        Debug.Log($"Filter: {selectedGameMode} players");

        foreach (var session in savedSessions)
        {
            if (!SessionMatchesSelectedGameMode(session, selectedGameMode))
                continue;

            activeSessions.Add(session.Name);

            bool isFull = session.PlayerCount >= session.MaxPlayers;
            int gameMode = GetSessionGameMode(session);

            GameMode sessionType = GameMode.Shared;

            if (session.Properties.TryGetValue("mode", out var type))
            {
                sessionType = (GameMode)(int)type.PropertyValue;
            }

            int currentPlayers = session.PlayerCount;
            int maxPlayers = session.MaxPlayers;

            if (sessionType == GameMode.Server)
            {
                currentPlayers--;
                maxPlayers--;
            }

            if (sessionButtons.TryGetValue(session.Name, out Button btn))
            {
                TMP_Text text = btn.GetComponentInChildren<TMP_Text>();

                text.text = $"{session.Name} {currentPlayers}/{maxPlayers}";

                btn.interactable = !isFull || !session.IsOpen;
            }
            else
            {
                if (session.Properties.TryGetValue("mode", out var mode))
                {
                    sessionType = (GameMode)(int)mode.PropertyValue;
                    CreateSessionUI(session.Name, session.MaxPlayers, session.PlayerCount, isFull, session.IsOpen, sessionType);
                }
            }
        }

        var keysToRemove = new List<string>();

        foreach (var kvp in sessionButtons)
        {
            if (!activeSessions.Contains(kvp.Key))
            {
                Destroy(kvp.Value.gameObject);
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
            sessionButtons.Remove(key);
    }

    private static bool SessionMatchesSelectedGameMode(SessionInfo session, int selectedGameMode)
    {
        return GetSessionGameMode(session) == selectedGameMode;
    }

    private static int GetSessionGameMode(SessionInfo session)
    {
        if (session.Properties != null &&
            session.Properties.TryGetValue(LobbyManager.GameModeSessionProperty, out SessionProperty gameMode))
        {
            return gameMode;
        }

        return session.MaxPlayers;
    }

    public void CreateErrorMessage(string message)
    {
        Instantiate(ErrorPrefab, baseObject.transform).GetComponent<ErrorMessage>().errorText.text = message;
    }
}
