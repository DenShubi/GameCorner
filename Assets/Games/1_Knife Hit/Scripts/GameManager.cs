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

    /// <summary>
    /// Hitung toughness berdasarkan level saat ini.
    /// </summary>
    private int GetToughnessForLevel()
    {
        return baseToughness + (toughnessPerLevel * (currentLevel - 1));
    }

    /// <summary>
    /// Hitung kecepatan rotasi berdasarkan level saat ini.
    /// </summary>
    private float GetRotationSpeedForLevel()
    {
        float speed = baseRotationSpeed + (rotationSpeedPerLevel * (currentLevel - 1));
        return Mathf.Min(speed, maxRotationSpeed);
    }

    /// <summary>
    /// Update tampilan level di UI.
    /// </summary>
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

            LogController logCtrl = newLog.GetComponent<LogController>();
            if (logCtrl != null)
            {
                logCtrl.toughness = GetToughnessForLevel();
                logCtrl.rotationSpeed = GetRotationSpeedForLevel();
            }

            Debug.Log($"[Level {currentLevel}] Toughness: {GetToughnessForLevel()}, " +
                      $"Rotation: {GetRotationSpeedForLevel()}");
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

    // ======= GETTER (untuk script lain yang perlu akses) =======
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentScore() => currentScore;
    public int GetCurrentHearts() => currentHearts;
}