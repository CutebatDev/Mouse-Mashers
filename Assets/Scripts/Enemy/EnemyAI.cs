using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;

    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float turnDelay = 2f;
    [SerializeField] private BoxCollider2D floor;

    private Vector2 target;
    private float timer;


     void Awake()
    {
        PickNewTarget();
    }

     void Update()
    {
        Vector2 current = transform.position;

        Vector2 dir = (target - current).normalized;

        transform.position += (Vector3)(dir * speed * Time.deltaTime);

        if (dir.x != 0)
            sprite.flipX = dir.x < 0;

        timer -= Time.deltaTime;

        if (Vector2.Distance(current, target) < 0.2f || timer <= 0)
        {
            PickNewTarget();
        }
    }

    private void PickNewTarget()
    {
        Bounds b = floor.bounds;

        target = new Vector2(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y)
        );

        timer = turnDelay;
    }
}
