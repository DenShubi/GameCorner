using UnityEngine;
using System.Collections.Generic;

public class KnifeSpawner : MonoBehaviour
{
    public GameObject knifePrefab;
    public Transform spawnPoint;

    [Header("Double Knife Settings")]
    [Tooltip("Jarak offset kiri-kanan antar 2 knife saat double knife aktif")]
    public float doubleKnifeOffset = 0.3f;

    private List<KnifeController> currentKnives = new List<KnifeController>();

    void Start() => SpawnNewKnife();

    void Update()
    {
        if (GameManager.instance.isGameOver) return;

        if (Input.GetMouseButtonDown(0) && currentKnives.Count > 0)
        {
            // Tembak semua knife yang ada (1 atau 2)
            foreach (KnifeController knife in currentKnives)
            {
                if (knife != null)
                {
                    knife.Shoot();
                }
            }
            currentKnives.Clear();

            // Konsumsi 1 charge double knife jika aktif
            GameManager.instance.ConsumeDoubleKnife();

            Invoke(nameof(SpawnNewKnife), 0.3f);
        }
    }

    void SpawnNewKnife()
    {
        if (GameManager.instance.isGameOver) return;
        if (knifePrefab == null || spawnPoint == null) return;

        currentKnives.Clear();

        if (GameManager.instance.IsDoubleKnifeActive())
        {
            // ===== DOUBLE KNIFE: spawn 2 knife berdampingan =====
            Vector3 leftPos = spawnPoint.position + Vector3.left * doubleKnifeOffset;
            Vector3 rightPos = spawnPoint.position + Vector3.right * doubleKnifeOffset;

            GameObject leftKnife = Instantiate(knifePrefab, leftPos, Quaternion.identity);
            GameObject rightKnife = Instantiate(knifePrefab, rightPos, Quaternion.identity);

            currentKnives.Add(leftKnife.GetComponent<KnifeController>());
            currentKnives.Add(rightKnife.GetComponent<KnifeController>());

            Debug.Log("[DoubleKnife] 2 knife spawned!");
            // ====================================================
        }
        else
        {
            // Normal: spawn 1 knife
            GameObject newKnife = Instantiate(knifePrefab, spawnPoint.position, Quaternion.identity);
            currentKnives.Add(newKnife.GetComponent<KnifeController>());
        }
    }

    /// <summary>
    /// Dipanggil oleh GameManager saat heart berkurang tapi game belum over.
    /// Memastikan knife baru spawn setelah gagal.
    /// </summary>
    public void ForceSpawnNewKnife()
    {
        CancelInvoke(nameof(SpawnNewKnife));
        Invoke(nameof(SpawnNewKnife), 0.5f);
    }
}