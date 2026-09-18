using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    private int score = 0;

    private void Start()
    {
        UpdateScore();
    }

    public void AddScore()
    {
        score++;
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }
}