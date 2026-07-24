using Fusion;
using UnityEngine;

public struct PlayerInputData : INetworkInput
{
    public NetworkBool IsPressed;
    public Vector2 WorldPosition;
}