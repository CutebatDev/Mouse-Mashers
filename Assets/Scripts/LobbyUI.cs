using Fusion;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LobbyManager lobbyManager;

    [SerializeField] private TextMeshProUGUI numberOfPlayersInSessionText;

    [SerializeField] private Button endSessionButton;

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject sessionPanel;
    [SerializeField] private GameObject bottomPanel;
    [SerializeField] private GameObject playerPanel;

    [SerializeField] private TMP_Dropdown maxPlayersDropdown;

    [SerializeField] private TextMeshProUGUI lobbyNameTextInput;
    [SerializeField] private TextMeshProUGUI roomNameTextInput;

    public string LobbyNameText => lobbyNameTextInput.text;
    public string RoomNameText => roomNameTextInput.text;
    public int RoomMaxPlayers => int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);

    [SerializeField] private Button sessionButton;

    private Dictionary<string, Button> sessionButtons = new();

    public void UpdatePlayerCount(int playerCount)
    {
        numberOfPlayersInSessionText.text = $"Players: {playerCount}";
    }

    public void UpdateUIState(NetState state)
    {
        playerPanel.SetActive(state == NetState.Lobby);
        menuPanel.SetActive(state == NetState.Disconnected);
        lobbyPanel.SetActive(state == NetState.Lobby);
        sessionPanel.SetActive(state == NetState.InSession);
        bottomPanel.SetActive(state == NetState.Lobby);
        endSessionButton.interactable = (state == NetState.InSession);
    }

    public void CreateSessionUI(string roomName, int maxPlayers, int currentPlayers, bool isFull)
    {
        Button btn = Instantiate(sessionButton, lobbyPanel.transform);

        btn.onClick.AddListener(() => lobbyManager.JoinRoom(roomName));

        btn.interactable = !isFull;

        TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
        text.text = $"{roomName} {currentPlayers}/{maxPlayers}";

        sessionButtons[roomName] = btn;
    }

    public void UpdateSessionPlayerCount(string roomName, int currentPlayerCount)
    {

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

        text.text = $"{roomName} {currentPlayerCount}/{maxPlayers}";
    }

    public void UpdateSessions(List<SessionInfo> sessions)
    {
        HashSet<string> activeSessions = new();

        foreach (var session in sessions)
        {
            activeSessions.Add(session.Name);

            bool isFull = session.PlayerCount >= session.MaxPlayers;

            if (sessionButtons.TryGetValue(session.Name, out Button btn))
            {
                TMP_Text text = btn.GetComponentInChildren<TMP_Text>();
                text.text = $"{session.Name} {session.PlayerCount}/{session.MaxPlayers}";

                btn.interactable = !isFull;
            }
            else
            {
                CreateSessionUI(session.Name, session.MaxPlayers, session.PlayerCount, isFull);
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
}
