using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : NetworkBehaviour
{
    [SerializeField] private Camera mainCam;

    Vector2 ScreenPosition => Mouse.current.position.ReadValue();

    public bool IsHoldingLMB => Mouse.current.leftButton.isPressed;
    
    public event Action QuitPressed;
    private InputAction quitAction;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
        {
            enabled = false;
            return;
        }
        
        if (!mainCam)
            mainCam = Camera.main;

        quitAction = new InputAction(binding: "<Keyboard>/q");
        quitAction.performed += _ => QuitPressed?.Invoke();
    }

    private void OnEnable() => quitAction.Enable();
    private void OnDisable() => quitAction.Disable();


    public Vector2 WorldPosition
    {
        get
        {
            Vector3 screenPos = Mouse.current.position.ReadValue();

            return mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));
        }
    }
}
