using UnityEngine;
using TMPro;

public class PongGameManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int maxScore = 7;

    [Header("UI References")]
    public TextMeshProUGUI scoreP1Text;
    public TextMeshProUGUI scoreP2Text;
    public GameObject gameOverPanel;
    public TextMeshProUGUI winnerText;

    [Header("References")]
    public PongBall ball;
    public PongCountdown countdown;

    private int scoreP1  = 0;
    private int scoreP2  = 0;
    private bool gameOver = false;

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateScoreUI();
    }

    public void OnGoal(int scoringPlayer)
    {
        if (gameOver) return;

        if (scoringPlayer == 1) scoreP1++;
        else                    scoreP2++;

        UpdateScoreUI();
        Debug.Log($"[Pong] Gol! Skor: P1={scoreP1} P2={scoreP2}");

        if (!CheckWinCondition())
        {
            // Belum ada pemenang → countdown ulang
            // Delay kecil agar pemain sempat lihat bola masuk gawang
            Invoke(nameof(TriggerCountdown), 0.5f);
        }
    }

    void TriggerCountdown()
    {
        countdown?.RestartCountdown();
    }

    bool CheckWinCondition()
    {
        if (scoreP1 >= maxScore) { EndGame(1); return true; }
        if (scoreP2 >= maxScore) { EndGame(2); return true; }
        return false;
    }

    void EndGame(int winner)
    {
        gameOver = true;

        ball?.StopBall();

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (winnerText    != null) winnerText.text = $"Player {winner}\nMenang! 🏆";

        Debug.Log($"[Pong] Player {winner} Menang!");
    }

    public void RestartGame()
    {
        scoreP1   = 0;
        scoreP2   = 0;
        gameOver  = false;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateScoreUI();
        countdown?.RestartCountdown();
    }

    void UpdateScoreUI()
    {
        if (scoreP1Text != null) scoreP1Text.text = scoreP1.ToString();
        if (scoreP2Text != null) scoreP2Text.text = scoreP2.ToString();
    }
}