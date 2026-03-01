using UnityEngine;

public class KnifeSpawner : MonoBehaviour
{
    public GameObject knifePrefab;
    public Transform spawnPoint;
    private KnifeController currentKnife;

    void Start() => SpawnNewKnife();

    void Update()
    {
        if (GameManager.instance.isGameOver) return;

        if (Input.GetMouseButtonDown(0) && currentKnife != null)
        {
            currentKnife.Shoot();
            currentKnife = null;
            Invoke(nameof(SpawnNewKnife), 0.3f);
        }
    }

    void SpawnNewKnife()
    {
        // Jangan spawn jika game over
        if (GameManager.instance.isGameOver) return;

        if (knifePrefab == null || spawnPoint == null) return;

        GameObject newKnife = Instantiate(knifePrefab, spawnPoint.position, Quaternion.identity);
        currentKnife = newKnife.GetComponent<KnifeController>();
    }

    /// <summary>
    /// Dipanggil oleh GameManager saat heart berkurang tapi game belum over.
    /// Memastikan knife baru spawn setelah gagal.
    /// </summary>
    public void ForceSpawnNewKnife()
    {
        // Cancel invoke sebelumnya jika ada
        CancelInvoke(nameof(SpawnNewKnife));

        // Spawn knife baru setelah delay pendek
        Invoke(nameof(SpawnNewKnife), 0.5f);
    }
}