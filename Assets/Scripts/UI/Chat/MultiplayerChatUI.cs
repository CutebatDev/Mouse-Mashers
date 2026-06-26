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

        usernameInput.text = "";
    }

    public void Send()
    {
        if (string.IsNullOrEmpty(input.text))
            return;

        string message = input.text;

        MultiplayerChat.Instance.SendMessage(username, message);

        input.text = "";

        input.ActivateInputField();
    }

    private void AddMessage(string user, string message)
    {
        _messages.text += $"{user}: {message}\n";
    }
}
