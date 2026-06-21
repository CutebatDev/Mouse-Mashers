using Fusion;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    private bool isReady = false;
    private SpriteRenderer sr;

    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.red;

    //[Networked]
    public bool IsReady => isReady;

    [Rpc]
    public void SetReadyRPC()
    {
        isReady = true;
        sr.color = readyColor;
    }

    [Rpc]
    public void SetUnreadyRPC()
    {
        isReady = false;
        sr.color = notReadyColor;
    }

    public override void Spawned()
    {
        base.Spawned();

        sr = GetComponent<SpriteRenderer>();
    }
}
