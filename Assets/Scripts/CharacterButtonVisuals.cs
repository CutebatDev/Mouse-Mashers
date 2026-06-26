using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonVisuals : MonoBehaviour
{
    [SerializeField] private int characterIndex;
    [SerializeField] private Button characterButton;
    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private CharacterSelectionManager manager;
    private bool isTaken;

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
        isTaken = taken;
        blockingPanel.SetActive(taken);
        characterButton.interactable = !taken;
    }

    public void SetInteractable(bool interactable)
    {
        characterButton.interactable = interactable && !isTaken;
    }
}
