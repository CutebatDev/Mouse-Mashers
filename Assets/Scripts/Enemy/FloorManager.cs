using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static BoxCollider2D Floor { get; private set; }

    void Awake()
    {
        Floor = GetComponent<BoxCollider2D>();
    }
}
