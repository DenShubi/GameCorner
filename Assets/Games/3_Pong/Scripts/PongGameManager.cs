using UnityEngine;
using TMPro;

/// <summary>
/// Mengatur skor dan state game Pong PvP.
/// </summary>
public class PongGameManager : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("Skor maksimum untuk menang")]
    public int maxScore = 7;

    [Header("UI References")]
    [Tooltip("Text skor P1")]
    public TextMeshProUGUI scoreP1Text;

    [Tooltip("Text skor P2")]
    public TextMeshProUGUI scoreP2Text;

    [Tooltip("Panel Game Over")]
    public GameObject gameOverPanel;

    [Tooltip("Text pemenang di panel Game Over")]
    public TextMeshProUGUI winnerText;

    [Header("References")]
    public PongBall ball;

    // Internal
    private int scoreP1 = 0;
    private int scoreP2 = 0;
    private bool gameOver = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateScoreUI();
    }

    /// <summary>
    /// Dipanggil oleh PongBall saat terjadi gol.
    /// </summary>
    /// <param name="scoringPlayer">Player yang dapat poin (1 atau 2)</param>
    public void OnGoal(int scoringPlayer)
    {
        if (gameOver) return;

        if (scoringPlayer == 1)
        {
            scoreP1++;
            Debug.Log($"[Pong] P1 Gol! Skor: P1={scoreP1} P2={scoreP2}");
        }
        else
        {
            scoreP2++;
            Debug.Log($"[Pong] P2 Gol! Skor: P1={scoreP1} P2={scoreP2}");
        }

        UpdateScoreUI();
        CheckWinCondition();
    }

    void UpdateScoreUI()
    {
        if (scoreP1Text != null) scoreP1Text.text = scoreP1.ToString();
        if (scoreP2Text != null) scoreP2Text.text = scoreP2.ToString();
    }

    void CheckWinCondition()
    {
        if (scoreP1 >= maxScore)
            EndGame(1);
        else if (scoreP2 >= maxScore)
            EndGame(2);
    }

    void EndGame(int winner)
    {
        gameOver = true;
        Debug.Log($"[Pong] Player {winner} Menang!");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (winnerText != null)
            winnerText.text = $"Player {winner}\nMenang! 🏆";

        if (ball != null)
        {
            var rb = ball.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = Vector3.zero;
        }
    }

    public void RestartGame()
    {
        scoreP1 = 0;
        scoreP2 = 0;
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateScoreUI();
        ball?.ForceReset();
    }
}