using TMPro;
using UnityEngine;

public class MultiplayerChatUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messages;
    [SerializeField] private TextMeshProUGUI input;
    [SerializeField]private TextMeshProUGUI usernameInput;

    private string username = "Rat";

    private void OnEnable()
    {
        MultiplayerChat.OnMessageReceived += AddMessage;
    }

    private void OnDisable()
    {
        MultiplayerChat.OnMessageReceived -= AddMessage;
    }

    public void SetUsername()
    {
        username = usernameInput.text;
    }

    public void Send()
    {
        MultiplayerChat.Instance.SendMessage(username, input.text);

        input.text = "";
    }

    private void AddMessage(string user, string message)
    {
        _messages.text += $"{user}: {message}\n";
    }
}
