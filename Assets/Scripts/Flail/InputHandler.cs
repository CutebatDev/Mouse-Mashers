using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : NetworkBehaviour
{
    [SerializeField] private Camera mainCam;

    Vector2 ScreenPosition => Mouse.current.position.ReadValue();

    public bool IsHoldingLMB => Mouse.current.leftButton.isPressed;

    public override void Spawned()
    {
        if (!mainCam)
            mainCam = Camera.main;
    }

    public Vector2 WorldPosition
    {
        get
        {
            Vector3 screenPos = Mouse.current.position.ReadValue();

            return mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z));
        }
    }
}
