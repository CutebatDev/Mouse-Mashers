using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterButtonVisuals : MonoBehaviour
{
    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private Button CharacterButton;
    public bool IsTaken { get; private set; } = false; 

    private void Start()
    {
        blockingPanel.SetActive(false);
    }

    public void ToggleTaken()
    {
        IsTaken = !IsTaken;
        blockingPanel.SetActive(IsTaken);
        CharacterButton.interactable = !IsTaken;
    }
}