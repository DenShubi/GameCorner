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
    public Transform logSpawnPoint; // Ini yang kosong di gambar Anda!
    public int maxKnivesOnLog = 10;

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

        // Bersihkan daftar pisau lama
        foreach (GameObject k in stuckKnives) { if (k != null) Destroy(k); }
        stuckKnives.Clear();

        currentLevelToughness += 2;
        Invoke(nameof(SpawnNewLog), 0.2f); // Jeda sedikit agar halus
    }

    void SpawnNewLog()
    {
        // Pengecekan keamanan agar tidak error lagi
        if (logPrefab != null && logSpawnPoint != null)
        {
            GameObject newLog = Instantiate(logPrefab, logSpawnPoint.position, Quaternion.Euler(90, 0, 0));
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
