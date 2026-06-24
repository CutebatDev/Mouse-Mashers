using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonVisuals : MonoBehaviour
{
    [SerializeField] private int characterIndex;
    [SerializeField] private Button characterButton;
    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private CharacterSelectionManager manager;

    private void Start()
    {
        SetTaken(false);
    }

    public void OnClicked()
    {
        Debug.Log($"OnClick Happened, , character ID {characterIndex}");
        manager.RequestPick(characterIndex);
    }

    public void SetTaken(bool taken)
    {
        Debug.Log($"SetTaken Happened, character ID {characterIndex}");
        blockingPanel.SetActive(taken);
        characterButton.interactable = !taken;
    }
}