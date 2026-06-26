using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MultiplayerChatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messages;
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TextMeshProUGUI nameplate;
    [SerializeField] private LobbyManager lobbyManager;

    private MultiplayerChat chat;

    private string username = "Rat";
    private bool usernameSent;

    private void Start()
    {
        nameplate.text = username;
        lobbyManager.SetLocalPlayerName(username);
    }

    private void OnEnable()
    {
        MultiplayerChat.OnMessageReceived += AddMessage;
    }

    private void OnDisable()
    {
        MultiplayerChat.OnMessageReceived -= AddMessage;
    }

    void Update()
    {
        if (chat == null)
            chat = MultiplayerChat.Instance;

        if (chat != null && !usernameSent)
        {
            chat.SetUsername(username);
            usernameSent = true;
        }

        //bool ready = MultiplayerChat.Instance != null;

        //usernameButton.interactable = ready;
        //sendButton.interactable = ready;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
            Send();
    }

    public void SetUsername()
    {
        string newUsername = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(newUsername))
            return;

        username = newUsername.Length > 16
            ? newUsername.Substring(0, 16)
            : newUsername;

        lobbyManager.SetLocalPlayerName(username);
        nameplate.text = username;

        usernameInput.text = "";

        if (chat == null)
            chat = MultiplayerChat.Instance;

        if (chat != null)
        {
            chat.SetUsername(username);
            usernameSent = true;
        }
        else
        {
            usernameSent = false;
        }
    }

    public void Send()
    {
        if (string.IsNullOrEmpty(input.text))
            return;

        string message = input.text;

        if (message.StartsWith("/whisper "))
        {
            SendWhisper(message);
        }
        else
        {
            chat.SendMessage(message);
        }

        input.text = "";
        input.ActivateInputField();
    }

    private void SendWhisper(string text)
    {
        string[] parts = text.Split(' ', 3);

        if (parts.Length < 3)
            return;

        string target = parts[1];
        string message = parts[2];

        chat.SendWhisper(target, message);
    }

    private void AddMessage(string user, string message)
    {
        _messages.text += $"{user}: {message}\n";
    }
}
