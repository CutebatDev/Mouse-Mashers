using Fusion;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    private bool isReady;
    private SpriteRenderer sr;

    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.red;

    public bool IsReady => isReady;

    [Rpc]
    public void ToggleReadyRPC()
    {
        isReady = !isReady;

        if (isReady) sr.color = readyColor;
        else sr.color = notReadyColor;
    }

    public override void Spawned()
    {
        base.Spawned();

        sr = GetComponent<SpriteRenderer>();
    }
}
