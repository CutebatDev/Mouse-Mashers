using Fusion;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color notReadyColor = Color.red;

    [Networked, OnChangedRender(nameof(OnReadyChanged))]
    public bool IsReady { get; set; }

    public void ToggleReady()
    {
        if (IsReady)
            SetUnreadyRPC();
        else
            SetReadyRPC();
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SetReadyRPC()
    {
        SetReady(true);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void SetUnreadyRPC()
    {
        SetReady(false);
    }

    public override void Spawned()
    {
        base.Spawned();

        sr = GetComponent<SpriteRenderer>();
        ApplyReadyVisual();
    }

    private void SetReady(bool isReady)
    {
        IsReady = isReady;
        ApplyReadyVisual();
    }

    private void OnReadyChanged()
    {
        ApplyReadyVisual();
    }

    private void ApplyReadyVisual()
    {
        if (sr == null)
            return;

        sr.color = IsReady ? readyColor : notReadyColor;
    }
}
