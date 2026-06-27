using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MultiplayerChatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messages;
    [SerializeField] private TMP_InputField input;

    // Lobby only
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TextMeshProUGUI nameplate;
    [SerializeField] private LobbyManager lobbyManager;

    private MultiplayerChat chat;

    private string username = "Rat";
    private bool usernameSent;

    private void Awake()
    {
        Debug.Log("Chat UI Awake");
    }

    private void Start()
    {
        Debug.Log("Chat UI Start");

        if (nameplate != null)
            nameplate.text = username;

        if (lobbyManager != null)
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


        // Wait until fusion finishes spawning NetworkBehaviour
        if (chat != null && chat.Object.IsValid && !usernameSent)
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

        if (usernameInput == null)
            return;


        string newUsername = usernameInput.text.Trim();

        if (string.IsNullOrEmpty(newUsername))
            return;

        username = newUsername.Length > 16
            ? newUsername.Substring(0, 16)
            : newUsername;

        if (lobbyManager!= null)
            lobbyManager.SetLocalPlayerName(username);

        if (nameplate != null)
            nameplate.text = username;

        usernameInput.text = "";

        if (chat == null)
            chat = MultiplayerChat.Instance;

        if (chat != null && chat.Object.IsValid)
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
        Debug.Log($"SEND CALLED. chat={chat}");

        if (chat == null)
        {
            Debug.Log("Chat is null");
            return;
        }

        if (chat.Object == null)
        {
            Debug.Log("Chat object is null");
            return;
        }

        if (!chat.Object.IsValid)
        {
            Debug.Log("Chat object invalid");
            return;
        }


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
        if (chat == null || !chat.Object.IsValid)
            return;


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
