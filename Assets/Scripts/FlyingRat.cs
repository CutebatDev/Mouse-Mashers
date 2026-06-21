using UnityEngine;

public class FlyingRat : MonoBehaviour
{
    [SerializeField] private GameObject startingPosition;
    [SerializeField] private GameObject targetPosition;
    [SerializeField] private float speed = 12;
    [SerializeField] private float distanceToConsiderReached = .1f;
    
    
    void Start()
    {
        transform.position = startingPosition.transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, targetPosition.transform.position) <= distanceToConsiderReached) {
            transform.position = startingPosition.transform.position;
        }
        
        transform.position = Vector3.MoveTowards(
            transform.position, targetPosition.transform.position, speed * Time.deltaTime
            );
    }
}
