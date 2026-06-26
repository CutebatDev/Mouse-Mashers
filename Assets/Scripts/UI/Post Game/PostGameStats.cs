using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PostGameStats : MonoBehaviour
{
    [SerializeField] private DetailsUI playerStatsPrefab;
    [SerializeField] private Transform root;
    [SerializeField] private string lobbySceneName = "Starting Screen";

    private bool returningToLobby;

    private void Start()
    {
        AudioManager.Instance.StopMusic();
        CreateUIDisplays();
    }

    private void CreateUIDisplays()
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            root.GetChild(i).gameObject.SetActive(false);
            Destroy(root.GetChild(i).gameObject);
        }

        SessionState sessionState = SessionState.Instance;

        if (sessionState == null)
        {
            Debug.LogError("Cannot create post-game stats: SessionState is missing.");
            return;
        }

        List<PlayerDetails> players = new();

        for (int i = 0; i < sessionState.Players.Length; i++)
        {
            PlayerDetails player = sessionState.Players[i];

            if (player.isActive && player.characterIndex >= 0)
                players.Add(player);
        }

        players.Sort((a, b) => b.score.CompareTo(a.score));

        if (players.Count == 0)
            return;

        int winningScore = players[0].score;

        foreach (PlayerDetails player in players)
        {
            DetailsUI details = Instantiate(playerStatsPrefab, root);

            details.SetName(player.playerName.ToString());
            details.SetRat(player.characterIndex);
            details.SetScore(player.score);

            if (player.score == winningScore)
                details.SetWinner();
        }
    }

    public async void ReturnToLobby()
    {
        if (returningToLobby)
            return;

        returningToLobby = true;

        string sceneName = lobbySceneName;
        NetworkRunner runner = SessionState.Instance != null
            ? SessionState.Instance.Runner
            : null;

        if (runner != null && runner.IsRunning)
            await runner.Shutdown();

        SceneManager.LoadScene(sceneName);
    }
}
