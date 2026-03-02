using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Manager untuk mode Multiplayer 1 Screen.
/// 2 Player berlomba mendapatkan score tertinggi sebelum timer habis.
/// Log di tengah, P1 dari bawah, P2 dari atas.
/// </summary>
public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager instance;

    [Header("UI - Player 1 (Bawah)")]
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p1NameText;

    [Header("UI - Player 2 (Atas)")]
    public TextMeshProUGUI p2ScoreText;
    public TextMeshProUGUI p2NameText;

    [Header("UI - Shared")]
    public TextMeshProUGUI timerText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Timer")]
    [Tooltip("Durasi permainan dalam detik")]
    public float matchDuration = 60f;

    [Header("Log Settings")]
    public GameObject logPrefab;
    public Transform logSpawnPoint;

    [Tooltip("Toughness log multiplayer (tetap, tidak naik level)")]
    public int logToughness = 8;

    [Tooltip("Kecepatan rotasi log")]
    public float logRotationSpeed = 120f;

    [Tooltip("Jumlah obstacle pada log")]
    public int obstacleCount = 2;

    [Header("Score Settings")]
    [Tooltip("Score saat knife menancap di log")]
    public int scorePerHit = 10;

    [Tooltip("Bonus score saat log hancur")]
    public int scorePerLogDestroy = 50;

    [Header("Knife Scatter")]
    public float knifeScatterForce = 8f;

    // Internal
    [HideInInspector] public bool isGameOver = false;
    private int p1Score = 0;
    private int p2Score = 0;
    private float timeRemaining;
    private List<GameObject> stuckKnives = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        timeRemaining = matchDuration;
        p1Score = 0;
        p2Score = 0;

        UpdateScoreUI();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        SpawnLog();
    }

    void Update()
    {
        if (isGameOver) return;

        // ===== COUNTDOWN TIMER =====
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndMatch();
        }

        UpdateTimerUI();
    }

    // ======= SCORE SYSTEM =======

    /// <summary>
    /// Tambah score untuk player tertentu.
    /// playerID: 1 = Player 1 (bawah), 2 = Player 2 (atas)
    /// </summary>
    public void AddScore(int playerID, int points)
    {
        if (isGameOver) return;

        if (playerID == 1)
        {
            p1Score += points;
        }
        else if (playerID == 2)
        {
            p2Score += points;
        }

        UpdateScoreUI();
        Debug.Log($"[Multi] P{playerID} +{points}! P1: {p1Score} | P2: {p2Score}");
    }

    // ============================

    // ======= LOG SYSTEM =======

    public void RegisterStuckKnife(GameObject knife)
    {
        stuckKnives.Add(knife);
    }

    public void LogDestroyed(int lastHitPlayerID)
    {
        // Bonus score ke player yang menghancurkan log
        AddScore(lastHitPlayerID, scorePerLogDestroy);

        ScatterStuckKnives();

        // Spawn log baru
        Invoke(nameof(SpawnLog), 0.5f);
    }

    void SpawnLog()
    {
        if (isGameOver) return;
        if (logPrefab == null || logSpawnPoint == null) return;

        GameObject newLog = Instantiate(logPrefab, logSpawnPoint.position, logSpawnPoint.rotation);

        // Ganti LogController dengan MultiplayerLogController jika ada
        // Atau setup LogController biasa
        MultiplayerLogController mpLog = newLog.GetComponent<MultiplayerLogController>();
        if (mpLog == null)
        {
            mpLog = newLog.AddComponent<MultiplayerLogController>();
        }

        mpLog.toughness = logToughness;
        mpLog.rotationSpeed = logRotationSpeed;

        // Spawn obstacles
        LogObstacleSpawner obsSpawner = newLog.GetComponent<LogObstacleSpawner>();
        if (obsSpawner != null && obstacleCount > 0)
        {
            obsSpawner.obstacleCount = obstacleCount;
            obsSpawner.SpawnObstacles();
        }

        Debug.Log($"[Multi] Log spawned! Toughness: {logToughness}, Speed: {logRotationSpeed}");
    }

    public void ScatterStuckKnives()
    {
        foreach (GameObject k in stuckKnives)
        {
            if (k != null)
            {
                k.transform.SetParent(null);

                Collider knifeCol = k.GetComponent<Collider>();
                if (knifeCol != null) knifeCol.enabled = false;

                Rigidbody rb = k.GetComponent<Rigidbody>();
                if (rb == null) rb = k.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;

                Vector3 scatterDir = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1.5f),
                    0f
                ).normalized;
                rb.AddForce(scatterDir * knifeScatterForce, ForceMode.Impulse);
                rb.AddTorque(new Vector3(0f, 0f, Random.Range(-10f, 10f)));

                Destroy(k, 3f);
            }
        }
        stuckKnives.Clear();
    }

    // ==========================

    // ======= TIMER & END =======

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateScoreUI()
    {
        if (p1ScoreText != null)
            p1ScoreText.text = p1Score.ToString();

        if (p2ScoreText != null)
            p2ScoreText.text = p2Score.ToString();
    }

    private void EndMatch()
    {
        isGameOver = true;

        Debug.Log($"[Multi] Match Over! P1: {p1Score} | P2: {p2Score}");

        // Tampilkan result
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            if (p1Score > p2Score)
            {
                resultText.text = $"🏆 PLAYER 1 WINS!\n\nP1: {p1Score}  |  P2: {p2Score}";
            }
            else if (p2Score > p1Score)
            {
                resultText.text = $"🏆 PLAYER 2 WINS!\n\nP1: {p1Score}  |  P2: {p2Score}";
            }
            else
            {
                resultText.text = $"🤝 DRAW!\n\nP1: {p1Score}  |  P2: {p2Score}";
            }
        }

        Invoke(nameof(RestartMatch), 5f);
    }

    void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ===========================

    // ======= GETTER =======
    public int GetP1Score() => p1Score;
    public int GetP2Score() => p2Score;
    public float GetTimeRemaining() => timeRemaining;
}