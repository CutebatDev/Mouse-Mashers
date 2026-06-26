using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerChatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messages;
    [SerializeField] private TMP_InputField input;
    [SerializeField]private TMP_InputField usernameInput;

    private string username = "Rat";

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
        if (Keyboard.current.enterKey.wasPressedThisFrame)
            Send();
    }

    public void SetUsername()
    {
        username = usernameInput.text;

        MultiplayerChat.Instance.SetUsername(username);

        usernameInput.text = "";
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
            MultiplayerChat.Instance.SendMessage(username, message);
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

        MultiplayerChat.Instance.SendWhisper(target, message);
    }

    private void AddMessage(string user, string message)
    {
        _messages.text += $"{user}: {message}\n";
    }
}
