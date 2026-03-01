using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;

    [Header("Heart System")]
    [Tooltip("Jumlah heart awal")]
    public int maxHearts = 3;

    [Tooltip("Referensi ke HeartUI script di Canvas")]
    public HeartUI heartUI;

    [Header("Level System")]
    [Tooltip("Toughness awal di level 1")]
    public int baseToughness = 5;

    [Tooltip("Tambahan toughness per level")]
    public int toughnessPerLevel = 2;

    [Tooltip("Kecepatan rotasi awal log")]
    public float baseRotationSpeed = 100f;

    [Tooltip("Tambahan kecepatan rotasi per level")]
    public float rotationSpeedPerLevel = 10f;

    [Tooltip("Kecepatan rotasi maksimum")]
    public float maxRotationSpeed = 300f;

    [Header("Obstacle System")]
    [Tooltip("Level mulai ada obstacle (level 1 = tanpa obstacle)")]
    public int obstacleStartLevel = 2;

    [Tooltip("Jumlah obstacle awal saat mulai muncul")]
    public int baseObstacles = 1;

    [Tooltip("Setiap berapa level, obstacle bertambah 1")]
    public int levelsPerExtraObstacle = 2;

    [Tooltip("Jumlah obstacle maksimum")]
    public int maxObstacles = 6;

    [Header("Settings")]
    public GameObject logPrefab;
    public Transform logSpawnPoint;
    public int maxKnivesOnLog = 10;

    [Header("Knife Scatter on Log Destroy")]
    [Tooltip("Kekuatan lontaran pisau saat log hancur")]
    public float knifeScatterForce = 8f;

    [HideInInspector] public bool isGameOver = false;

    private int currentScore = 0;
    private int currentHearts;
    private int currentLevel = 1;
    private List<GameObject> stuckKnives = new List<GameObject>();

    private bool heartCooldown = false;

    void Awake() => instance = this;

    void Start()
    {
        currentHearts = maxHearts;
        currentLevel = 1;

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHearts);
        }

        UpdateLevelUI();
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;
        currentScore += points;
        scoreText.text = currentScore.ToString();
    }

    public void RegisterStuckKnife(GameObject knife)
    {
        stuckKnives.Add(knife);
        if (stuckKnives.Count > maxKnivesOnLog)
        {
            GameObject oldest = stuckKnives[0];
            stuckKnives.RemoveAt(0);
            Destroy(oldest);
        }
    }

    public void LoseHeart()
    {
        if (isGameOver) return;
        if (heartCooldown) return;

        heartCooldown = true;

        currentHearts--;
        Debug.Log($"[Heart] Sisa heart: {currentHearts}/{maxHearts}");

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHearts);
        }

        if (currentHearts <= 0)
        {
            TriggerGameOver();
        }
        else
        {
            KnifeSpawner spawner = FindObjectOfType<KnifeSpawner>();
            if (spawner != null)
            {
                spawner.ForceSpawnNewKnife();
            }

            Invoke(nameof(ResetHeartCooldown), 0.5f);
        }
    }

    private void ResetHeartCooldown()
    {
        heartCooldown = false;
    }

    public void LogDestroyed()
    {
        AddScore(50);

        // ======= PISAU IKUT TERPENTAL =======
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
        // =====================================

        // ======= LEVEL UP =======
        currentLevel++;
        UpdateLevelUI();
        Debug.Log($"[Level] Naik ke Level {currentLevel}!");
        // =========================

        Invoke(nameof(SpawnNewLog), 0.5f);
    }

    private int GetToughnessForLevel()
    {
        return baseToughness + (toughnessPerLevel * (currentLevel - 1));
    }

    private float GetRotationSpeedForLevel()
    {
        float speed = baseRotationSpeed + (rotationSpeedPerLevel * (currentLevel - 1));
        return Mathf.Min(speed, maxRotationSpeed);
    }

    /// <summary>
    /// Hitung jumlah obstacle berdasarkan level.
    /// </summary>
    private int GetObstacleCountForLevel()
    {
        if (currentLevel < obstacleStartLevel) return 0;

        int levelsWithObstacles = currentLevel - obstacleStartLevel;
        int count = baseObstacles + (levelsWithObstacles / levelsPerExtraObstacle);
        return Mathf.Min(count, maxObstacles);
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "Level " + currentLevel;
        }
    }

    void SpawnNewLog()
    {
        if (logPrefab != null && logSpawnPoint != null)
        {
            GameObject newLog = Instantiate(logPrefab, logSpawnPoint.position, logSpawnPoint.rotation);

            // Set toughness dan rotation speed
            LogController logCtrl = newLog.GetComponent<LogController>();
            if (logCtrl != null)
            {
                logCtrl.toughness = GetToughnessForLevel();
                logCtrl.rotationSpeed = GetRotationSpeedForLevel();
            }

            // ======= SPAWN OBSTACLES =======
            int obstacleCount = GetObstacleCountForLevel();
            LogObstacleSpawner obsSpawner = newLog.GetComponent<LogObstacleSpawner>();
            if (obsSpawner != null && obstacleCount > 0)
            {
                obsSpawner.obstacleCount = obstacleCount;
                obsSpawner.SpawnObstacles();
            }
            // ================================

            Debug.Log($"[Level {currentLevel}] Toughness: {GetToughnessForLevel()}, " +
                      $"Rotation: {GetRotationSpeedForLevel()}, " +
                      $"Obstacles: {obstacleCount}");
        }
        else
        {
            Debug.LogError("Gagal Respawn: Log Prefab atau Spawn Point belum diisi di Inspector!");
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log($"[GameOver] Game Over! Final Level: {currentLevel}, Score: {currentScore}");
        Invoke(nameof(RestartGame), 2f);
    }

    void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    // ======= GETTER =======
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentScore() => currentScore;
    public int GetCurrentHearts() => currentHearts;
}