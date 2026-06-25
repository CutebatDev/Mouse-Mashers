using Fusion;
using UnityEngine;

public class CreateCharacterButton : NetworkBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject buttonParent;
    public void SpawnButton(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer == player)
            runner.Spawn(buttonPrefab, Vector3.zero, Quaternion.identity).transform.SetParent(buttonParent.transform);
    }
}