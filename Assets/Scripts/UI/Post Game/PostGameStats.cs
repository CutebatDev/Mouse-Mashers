using UnityEngine;

public class PostGameStats : MonoBehaviour
{
    [SerializeField] private DetailsUI playerStatsPrefab;
    [SerializeField] private Transform root;

    [SerializeField] private PlayerDetails[] detailsArr;

    //public void ReceivePlayerDetails()
    //{
    //    PlayerDetails[] array = someManager.Players;

    //    for (int i = 0; i < array.Length - 1; i++)
    //    {
    //        detailsArr[i] = array[i];
    //    }
    //}

    public void CreateUIDisplays()
    {
        PlayerDetails current = detailsArr[0];

        PlayerDetails winner = detailsArr[0];

        for (int i = 0; i < detailsArr.Length - 1; i++)
        {
            current = detailsArr[i];

            DetailsUI details = Instantiate(playerStatsPrefab, root.transform.position, Quaternion.identity, root);

            details.SetName(current.playerName);
            details.SetRat(current.characterIndex);
            details.SetScore(current.score);

            if (i == 0)
                details.SetWinner();

            if (current.score > winner.score)
                details.SetWinner();
        }
    }
}
