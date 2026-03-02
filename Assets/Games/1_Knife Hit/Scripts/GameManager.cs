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

    [Tooltip("Prefab power-up Score Multiplier")]
    public GameObject powerUpScoreMultiplierPrefab;

    [Tooltip("Prefab power-up Shield")]
    public GameObject powerUpShieldPrefab;

    [Tooltip("Prefab power-up Double Knife")]
    public GameObject powerUpDoubleKnifePrefab;

    [Tooltip("Persentase kemungkinan power-up muncul per log (0-100)")]
    [Range(0, 100)]
    public int powerUpChance = 30;

    [Tooltip("Jarak power-up dari pusat log (fallback jika tidak ada LogObstacleSpawner)")]
    public float powerUpDistance = 0.5f;

    [Header("Boss System")]
    [Tooltip("Prefab Boss Log berlapis")]
    public GameObject bossLogPrefab;

    [Tooltip("Setiap berapa level boss muncul")]
    public int bossEveryNLevels = 5;

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

    // ======= SCORE MULTIPLIER =======
    private int scoreMultiplier = 1;
    // ================================

    // ======= SHIELD =======
    private int shieldCharges = 0;
    // =======================

    // ======= DOUBLE KNIFE =======
    private int doubleKnifeRemaining = 0;
    // ============================

    // ======= BOSS TRACKING =======
    private bool isBossLevel = false;
    private GameObject currentBossObject;
    // =============================

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

        int finalPoints = points * scoreMultiplier;
        currentScore += finalPoints;
        scoreText.text = currentScore.ToString();

        if (scoreMultiplier > 1)
        {
            Debug.Log($"[ScoreMultiplier] {points} x{scoreMultiplier} = {finalPoints}");
        }
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

    public void ActivateDoubleHit(int hitCount)
    {
        doubleHitRemaining += hitCount;
        Debug.Log($"[DoubleHit] Aktif! Sisa hit double: {doubleHitRemaining}");
    }

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

    // ======= SCORE MULTIPLIER SYSTEM =======

    public void ActivateScoreMultiplier(int multiplier, float duration)
    {
        CancelInvoke(nameof(ResetScoreMultiplier));

        scoreMultiplier = multiplier;
        Debug.Log($"[ScoreMultiplier] Aktif! Score x{scoreMultiplier} selama {duration}s");

        UpdateScoreUI();

        Invoke(nameof(ResetScoreMultiplier), duration);
    }

    private void ResetScoreMultiplier()
    {
        scoreMultiplier = 1;
        Debug.Log("[ScoreMultiplier] Kembali normal x1");

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText == null) return;

        if (scoreMultiplier > 1)
        {
            scoreText.text = currentScore + " x" + scoreMultiplier;
        }
        else
        {
            scoreText.text = currentScore.ToString();
        }
    }

    // =======================================

    // ======= SHIELD SYSTEM =======

    public void ActivateShield(int charges)
    {
        shieldCharges += charges;
        Debug.Log($"[Shield] Aktif! Charges: {shieldCharges}");
    }

    public bool TryUseShield()
    {
        if (shieldCharges > 0)
        {
            shieldCharges--;
            Debug.Log($"[Shield] Hit diserap! Sisa charges: {shieldCharges}");
            return true;
        }
        return false;
    }

    // =============================

    // ======= DOUBLE KNIFE SYSTEM =======

    public void ActivateDoubleKnife(int throwCount)
    {
        doubleKnifeRemaining += throwCount;
        Debug.Log($"[DoubleKnife] Aktif! Sisa throw double: {doubleKnifeRemaining}");
    }

    public bool IsDoubleKnifeActive()
    {
        return doubleKnifeRemaining > 0;
    }

    public void ConsumeDoubleKnife()
    {
        if (doubleKnifeRemaining > 0)
        {
            doubleKnifeRemaining--;
            Debug.Log($"[DoubleKnife] Throw double! Sisa: {doubleKnifeRemaining}");
        }
    }

    // ===================================

    public void LoseHeart()
    {
        if (isGameOver) return;
        if (heartCooldown) return;

        heartCooldown = true;

        if (TryUseShield())
        {
            Debug.Log("[Shield] Shield melindungi! Heart tidak berkurang.");

            KnifeSpawner spawner = FindObjectOfType<KnifeSpawner>();
            if (spawner != null)
            {
                spawner.ForceSpawnNewKnife();
            }

            Invoke(nameof(ResetHeartCooldown), 0.5f);
            return;
        }

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

        ScatterStuckKnives();

        currentLevel++;
        UpdateLevelUI();
        Debug.Log($"[Level] Naik ke Level {currentLevel}!");

        Invoke(nameof(SpawnNewLog), 0.5f);
    }

    public void BossDefeated()
    {
        isBossLevel = false;

        if (currentBossObject != null)
        {
            Destroy(currentBossObject);
            currentBossObject = null;
        }

        currentLevel++;
        UpdateLevelUI();
        Debug.Log($"[Boss] Boss defeated! Naik ke Level {currentLevel}!");

        Invoke(nameof(SpawnNewLog), 1f);
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
            if (isBossLevel)
            {
                levelText.text = "⚔ BOSS Level " + currentLevel + " ⚔";
            }
            else
            {
                levelText.text = "Level " + currentLevel;
            }
        }
    }

    private bool IsBossLevel()
    {
        return currentLevel % bossEveryNLevels == 0;
    }

    void SpawnNewLog()
    {
        if (IsBossLevel() && bossLogPrefab != null)
        {
            SpawnBossLog();
            return;
        }

        if (logPrefab != null && logSpawnPoint != null)
        {
            GameObject newLog = Instantiate(logPrefab, logSpawnPoint.position, logSpawnPoint.rotation);

            LogController logCtrl = newLog.GetComponent<LogController>();
            if (logCtrl != null)
            {
                logCtrl.toughness = GetToughnessForLevel();
                logCtrl.rotationSpeed = GetRotationSpeedForLevel();
            }

            int obstacleCount = GetObstacleCountForLevel();
            LogObstacleSpawner obsSpawner = newLog.GetComponent<LogObstacleSpawner>();
            if (obsSpawner != null && obstacleCount > 0)
            {
                obsSpawner.obstacleCount = obstacleCount;
                obsSpawner.SpawnObstacles();
            }

            SpawnPowerUpOnLog(newLog);

            Debug.Log($"[Level {currentLevel}] Toughness: {GetToughnessForLevel()}, " +
                      $"Rotation: {GetRotationSpeedForLevel()}, " +
                      $"Obstacles: {obstacleCount}");
        }
        else
        {
            Debug.LogError("Gagal Respawn: Log Prefab atau Spawn Point belum diisi di Inspector!");
        }
    }

    private void SpawnBossLog()
    {
        isBossLevel = true;
        UpdateLevelUI();

        currentBossObject = Instantiate(bossLogPrefab, logSpawnPoint.position, logSpawnPoint.rotation);

        BossLogController bossCtrl = currentBossObject.GetComponent<BossLogController>();
        if (bossCtrl != null)
        {
            int bossNumber = currentLevel / bossEveryNLevels;
            bossCtrl.InitBoss(bossNumber);
        }

        Debug.Log($"[Boss] Boss Level {currentLevel}! Boss #{currentLevel / bossEveryNLevels}");
    }

    /// <summary>
    /// Spawn power-up pada log. Public agar BossLogController bisa panggil.
    /// Menggunakan handleDistance dari LogObstacleSpawner jika ada,
    /// dan lossyScale untuk kompensasi scale yang benar.
    /// </summary>
    public void SpawnPowerUpOnLog(GameObject log)
    {
        List<GameObject> availablePowerUps = new List<GameObject>();
        if (powerUpTimeSlowPrefab != null) availablePowerUps.Add(powerUpTimeSlowPrefab);
        if (powerUpDoubleHitPrefab != null) availablePowerUps.Add(powerUpDoubleHitPrefab);
        if (powerUpScoreMultiplierPrefab != null) availablePowerUps.Add(powerUpScoreMultiplierPrefab);
        if (powerUpShieldPrefab != null) availablePowerUps.Add(powerUpShieldPrefab);
        if (powerUpDoubleKnifePrefab != null) availablePowerUps.Add(powerUpDoubleKnifePrefab);

        if (availablePowerUps.Count == 0) return;

        int roll = Random.Range(0, 100);
        if (roll >= powerUpChance) return;

        GameObject chosenPrefab = availablePowerUps[Random.Range(0, availablePowerUps.Count)];

        // ===== FIX: Gunakan handleDistance dari LogObstacleSpawner jika ada =====
        float distance = powerUpDistance;
        LogObstacleSpawner obsSpawner = log.GetComponent<LogObstacleSpawner>();
        if (obsSpawner != null)
        {
            distance = obsSpawner.handleDistance;
        }
        // ========================================================================

        float angle = Random.Range(0f, 360f);
        float rad = angle * Mathf.Deg2Rad;

        Vector3 localPos = new Vector3(
            Mathf.Sin(rad) * distance,
            Mathf.Cos(rad) * distance,
            0f
        );

        GameObject powerUp = Instantiate(chosenPrefab, log.transform);
        powerUp.transform.localPosition = localPos;

        // ===== FIX: Gunakan lossyScale untuk kompensasi yang benar =====
        Vector3 worldScale = log.transform.lossyScale;
        Vector3 prefabScale = chosenPrefab.transform.localScale;
        powerUp.transform.localScale = new Vector3(
            prefabScale.x / worldScale.x,
            prefabScale.y / worldScale.y,
            prefabScale.z / worldScale.z
        );
        // ================================================================

        Debug.Log($"[PowerUp] {chosenPrefab.name} spawned on {log.name} at angle {angle:F0}°, distance {distance}");
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
    public int GetShieldCharges() => shieldCharges;
    public int GetDoubleKnifeRemaining() => doubleKnifeRemaining;
    public bool GetIsBossLevel() => isBossLevel;
}