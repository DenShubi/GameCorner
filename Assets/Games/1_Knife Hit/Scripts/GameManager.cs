using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Heart System")]
    [Tooltip("Jumlah heart awal")]
    public int maxHearts = 3;

    [Tooltip("Referensi ke HeartUI script di Canvas")]
    public HeartUI heartUI;

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
    private int currentLevelToughness = 5;
    private List<GameObject> stuckKnives = new List<GameObject>();

    // Cooldown agar LoseHeart tidak dipanggil berkali-kali sekaligus
    private bool heartCooldown = false;

    void Awake() => instance = this;

    void Start()
    {
        currentHearts = maxHearts;

        if (heartUI != null)
        {
            heartUI.UpdateHearts(currentHearts);
        }
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

    /// <summary>
    /// Dipanggil saat knife mengenai obstacle (knife lain).
    /// Menggunakan cooldown agar tidak double-trigger.
    /// </summary>
    public void LoseHeart()
    {
        if (isGameOver) return;
        if (heartCooldown) return; // Cegah double trigger

        heartCooldown = true;

        currentHearts--;
        Debug.Log($"[Heart] Sisa heart: {currentHearts}/{maxHearts}");

        // Update UI
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
            // Heart masih ada → spawn knife baru
            KnifeSpawner spawner = FindObjectOfType<KnifeSpawner>();
            if (spawner != null)
            {
                spawner.ForceSpawnNewKnife();
            }

            // Reset cooldown setelah delay singkat
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

        currentLevelToughness += 2;
        Invoke(nameof(SpawnNewLog), 0.5f);
    }

    void SpawnNewLog()
    {
        if (logPrefab != null && logSpawnPoint != null)
        {
            GameObject newLog = Instantiate(logPrefab, logSpawnPoint.position, logSpawnPoint.rotation);
            newLog.GetComponent<LogController>().toughness = currentLevelToughness;
        }
        else
        {
            Debug.LogError("Gagal Respawn: Log Prefab atau Spawn Point belum diisi di Inspector!");
        }
    }

    public void TriggerGameOver()
    {
        isGameOver = true;
        Debug.Log("[GameOver] Game Over!");
        Invoke(nameof(RestartGame), 2f);
    }

    void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}