using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI scoreLabel;
    [SerializeField] private GameObject crown;
    [SerializeField] private Image playerRat;

    [SerializeField] private Sprite[] allRats;

    [SerializeField] private string scoreTextPrefix = "Score: ";

    public void SetName(string name)
    {
        nameLabel.text = name;
    }

    public void SetScore(int score)
    {
        scoreLabel.text = scoreTextPrefix + score;
    }

    public void SetWinner()
    {
        crown.SetActive(true);
    }

    public void SetRat(int index)
    {
        if (index < 0 || index >= allRats.Length)
            return;

        playerRat.sprite = allRats[index];
    }
}
