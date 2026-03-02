using UnityEngine;

/// <summary>
/// Spawn obstacle (pisau yang sudah menancap) pada log saat pertama kali muncul.
/// Pivot knife ada di handle, jadi kita posisikan handle di luar log
/// dan arahkan blade ke pusat log.
/// Menggunakan lossyScale untuk menghitung scale yang benar di nested parent (boss).
/// </summary>
public class LogObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Prefab knife yang akan dijadikan obstacle")]
    public GameObject obstaclePrefab;

    [Tooltip("Jarak handle knife dari pusat log (obstacle muncul di sini)")]
    public float handleDistance = 0.7f;

    [Tooltip("Jumlah obstacle yang akan di-spawn (diatur oleh GameManager)")]
    [HideInInspector] public int obstacleCount = 0;

    [Tooltip("Sudut minimum antar obstacle (agar tidak terlalu rapat)")]
    public float minAngleBetween = 40f;

    public void SpawnObstacles()
    {
        if (obstaclePrefab == null)
        {
            Debug.LogWarning("[Obstacle] obstaclePrefab belum diisi di Inspector!");
            return;
        }

        if (obstacleCount <= 0) return;

        // Scale prefab knife asli (world space)
        Vector3 knifeWorldScale = obstaclePrefab.transform.localScale;

        // ===== FIX: Gunakan lossyScale (world scale) untuk kompensasi =====
        // lossyScale memperhitungkan semua parent (termasuk BossLog parent)
        Vector3 worldScale = transform.lossyScale;
        Vector3 correctedScale = new Vector3(
            knifeWorldScale.x / worldScale.x,
            knifeWorldScale.y / worldScale.y,
            knifeWorldScale.z / worldScale.z
        );
        // ==================================================================

        float[] angles = GenerateSpacedAngles(obstacleCount, minAngleBetween);

        for (int i = 0; i < obstacleCount; i++)
        {
            float angle = angles[i];
            float rad = angle * Mathf.Deg2Rad;

            Vector3 outwardDir = new Vector3(
                Mathf.Sin(rad),
                Mathf.Cos(rad),
                0f
            );

            Vector3 localPos = outwardDir * handleDistance;

            GameObject obstacle = Instantiate(obstaclePrefab, transform);

            obstacle.transform.localPosition = localPos;

            // ===== FIX SCALE: kompensasi world scale =====
            obstacle.transform.localScale = correctedScale;

            // ===== ROTASI: Blade mengarah ke pusat log =====
            float rotZ = Mathf.Atan2(outwardDir.x, outwardDir.y) * Mathf.Rad2Deg;
            obstacle.transform.localRotation = Quaternion.Euler(0f, 0f, -rotZ + 180f);

            obstacle.tag = "Knife";

            Collider col = obstacle.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            Rigidbody rb = obstacle.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            KnifeController kc = obstacle.GetComponent<KnifeController>();
            if (kc != null)
            {
                Destroy(kc);
            }
        }

        Debug.Log($"[Obstacle] Spawned {obstacleCount} obstacles. " +
                  $"World scale: {worldScale}, Corrected knife scale: {correctedScale}");
    }

    private float[] GenerateSpacedAngles(int count, float minAngle)
    {
        float[] angles = new float[count];
        float spacing = 360f / count;

        if (spacing < minAngle && count > 1)
        {
            spacing = minAngle;
        }

        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            float randomOffset = Random.Range(-spacing * 0.2f, spacing * 0.2f);
            angles[i] = (startAngle + (i * spacing) + randomOffset) % 360f;
        }

        return angles;
    }
}