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
        manager.RequestPick(characterIndex);
    }

    public void SetTaken(bool taken)
    {
        blockingPanel.SetActive(taken);
        characterButton.interactable = !taken;
    }
}