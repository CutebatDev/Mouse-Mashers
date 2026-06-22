using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandleController : NetworkBehaviour
{
    [SerializeField] private InputHandler input;
    [SerializeField] private Rigidbody2D handleRb;
    [SerializeField] private NetworkObject networkObject;

    [SerializeField] private float handleSpeed = 10f;

    private Vector2 targetPosition;

    void Update()
    {
        if(!networkObject.HasStateAuthority)
            return;
        if (input.IsHoldingLMB)
            targetPosition = Vector2.MoveTowards(handleRb.position, input.WorldPosition, handleSpeed * Time.fixedDeltaTime);
    }

    public override void Spawned()
    {
        base.Spawned();
        Debug.Log($"Has input authority?? - {networkObject.HasInputAuthority}");
    }

    public override void FixedUpdateNetwork()
    {
        handleRb.MovePosition(targetPosition);
    }
}
