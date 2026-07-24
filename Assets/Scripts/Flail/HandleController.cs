using Fusion;
using UnityEngine;

public class HandleController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D handleRb;
    [SerializeField] private float handleSpeed = 10f;

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out PlayerInputData input))
            return;

        if (!input.IsPressed)
            return;

        Vector2 targetPosition = Vector2.MoveTowards(
            handleRb.position,
            input.WorldPosition,
            handleSpeed * Runner.DeltaTime
        );

        handleRb.MovePosition(targetPosition);
    }
}