using UnityEngine;
using TMPro;

/// <summary>
/// Menyimpan dan menampilkan highest score menggunakan PlayerPrefs.
/// Simpan lokal di device.
/// </summary>
public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager instance;

    [Header("UI")]
    [Tooltip("Text untuk menampilkan high score (di samping trophy icon)")]
    public TextMeshProUGUI highScoreText;

    private const string HIGH_SCORE_KEY = "KnifeHit_HighScore";
    private int highScore = 0;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Load high score dari PlayerPrefs
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateHighScoreUI();

        Debug.Log($"[HighScore] Loaded: {highScore}");
    }

    /// <summary>
    /// Cek apakah score saat ini lebih tinggi dari high score.
    /// Dipanggil setiap kali score bertambah atau game over.
    /// </summary>
    public void TryUpdateHighScore(int currentScore)
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();

            UpdateHighScoreUI();

            Debug.Log($"[HighScore] NEW HIGH SCORE: {highScore}!");
        }
    }

    /// <summary>
    /// Update tampilan high score di UI.
    /// </summary>
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
    }

    /// <summary>
    /// Getter untuk high score saat ini.
    /// </summary>
    public int GetHighScore() => highScore;

    /// <summary>
    /// Reset high score (untuk debug).
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, 0);
        PlayerPrefs.Save();
        UpdateHighScoreUI();
        Debug.Log("[HighScore] Reset to 0");
    }
}