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

    [Header("Power-Up System")]
    [Tooltip("Prefab power-up Time Slow")]
    public GameObject powerUpTimeSlowPrefab;

    [Tooltip("Prefab power-up Double Hit")]
    public GameObject powerUpDoubleHitPrefab;

    [Tooltip("Persentase kemungkinan power-up muncul per log (0-100)")]
    [Range(0, 100)]
    public int powerUpChance = 30;

    [Tooltip("Jarak power-up dari pusat log")]
    public float powerUpDistance = 0.5f;

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

    // ======= DOUBLE HIT =======
    private int doubleHitRemaining = 0;
    // ===========================

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

    // ======= DOUBLE HIT SYSTEM =======

    /// <summary>
    /// Aktifkan double hit. Dipanggil oleh PowerUpDoubleHit.
    /// </summary>
    public void ActivateDoubleHit(int hitCount)
    {
        doubleHitRemaining += hitCount;
        Debug.Log($"[DoubleHit] Aktif! Sisa hit double: {doubleHitRemaining}");
    }

    /// <summary>
    /// Ambil damage untuk knife saat ini. Jika double hit aktif, return 2.
    /// Dipanggil oleh KnifeController saat kena log.
    /// </summary>
    public int GetKnifeDamage()
    {
        if (doubleHitRemaining > 0)
        {
            doubleHitRemaining--;
            Debug.Log($"[DoubleHit] Damage x2! Sisa: {doubleHitRemaining}");
            return 2;
        }
        return 1;
    }

    // ==================================

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

            // ======= SPAWN POWER-UP (random chance) =======
            SpawnPowerUpOnLog(newLog);
            // ===============================================

            Debug.Log($"[Level {currentLevel}] Toughness: {GetToughnessForLevel()}, " +
                      $"Rotation: {GetRotationSpeedForLevel()}, " +
                      $"Obstacles: {obstacleCount}");
        }
        else
        {
            Debug.LogError("Gagal Respawn: Log Prefab atau Spawn Point belum diisi di Inspector!");
        }
    }

    /// <summary>
    /// Spawn power-up pada log. Random pilih antara Time Slow atau Double Hit.
    /// </summary>
    private void SpawnPowerUpOnLog(GameObject log)
    {
        // Kumpulkan prefab yang tersedia
        List<GameObject> availablePowerUps = new List<GameObject>();
        if (powerUpTimeSlowPrefab != null) availablePowerUps.Add(powerUpTimeSlowPrefab);
        if (powerUpDoubleHitPrefab != null) availablePowerUps.Add(powerUpDoubleHitPrefab);

        if (availablePowerUps.Count == 0) return;

        // Random chance
        int roll = Random.Range(0, 100);
        if (roll >= powerUpChance) return;

        // Pilih power-up secara acak
        GameObject chosenPrefab = availablePowerUps[Random.Range(0, availablePowerUps.Count)];

        // Posisi acak di sekitar log
        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;

        Vector3 localPos = new Vector3(
            Mathf.Sin(rad) * powerUpDistance,
            Mathf.Cos(rad) * powerUpDistance,
            0f
        );

        // Spawn sebagai child log (ikut berputar)
        GameObject powerUp = Instantiate(chosenPrefab, log.transform);
        powerUp.transform.localPosition = localPos;

        // Kompensasi scale parent log
        Vector3 logScale = log.transform.localScale;
        Vector3 prefabScale = chosenPrefab.transform.localScale;
        powerUp.transform.localScale = new Vector3(
            prefabScale.x / logScale.x,
            prefabScale.y / logScale.y,
            prefabScale.z / logScale.z
        );

        Debug.Log($"[PowerUp] {chosenPrefab.name} spawned at angle {angle:F0}°");
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