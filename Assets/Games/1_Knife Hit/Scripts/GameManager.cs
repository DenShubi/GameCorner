using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Settings")]
    public GameObject logPrefab;
    public Transform logSpawnPoint;
    public int maxKnivesOnLog = 10;

    [Header("Knife Scatter on Log Destroy")]
    [Tooltip("Kekuatan lontaran pisau saat log hancur")]
    public float knifeScatterForce = 8f;

    [HideInInspector] public bool isGameOver = false;

    private int currentScore = 0;
    private int currentLevelToughness = 5;
    private List<GameObject> stuckKnives = new List<GameObject>();

    void Awake() => instance = this;

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

    public void LogDestroyed()
    {
        AddScore(50);

        // ======= PISAU IKUT TERPENTAL =======
        foreach (GameObject k in stuckKnives)
        {
            if (k != null)
            {
                // Lepaskan dari parent (log yang akan dihancurkan)
                k.transform.SetParent(null);

                // Matikan collider agar tidak trigger game over
                Collider knifeCol = k.GetComponent<Collider>();
                if (knifeCol != null) knifeCol.enabled = false;

                // Tambahkan Rigidbody agar bisa terpental
                Rigidbody rb = k.GetComponent<Rigidbody>();
                if (rb == null) rb = k.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;

                // Beri arah acak ke atas
                Vector3 scatterDir = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1.5f),
                    Random.Range(-0.5f, 0.5f)
                ).normalized;
                rb.AddForce(scatterDir * knifeScatterForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 10f);

                // Hapus pisau setelah beberapa detik
                Destroy(k, 3f);
            }
        }
        stuckKnives.Clear();
        // =====================================

        currentLevelToughness += 2;
        Invoke(nameof(SpawnNewLog), 0.5f); // Sedikit delay lebih lama agar efek terlihat
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
        Invoke(nameof(RestartGame), 2f);
    }

    void RestartGame() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}