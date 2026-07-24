using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalControls : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private InputActionReference exitAction;

    void OnEnable()
    {
        exitAction.action.performed += ReturnToLobby;    
    }

    void OnDisable()
    {
        exitAction.action.performed -= ReturnToLobby;    
    }

    public void ReturnToLobby(InputAction.CallbackContext context)
    {
        gameManager.ActivateReturnToMenu();
    }
}
