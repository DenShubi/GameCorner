using UnityEngine;

/// <summary>
/// Nerf Faster Log. Menempel di log, ikut berputar.
/// Saat knife player mengenai nerf ini, log akan berputar lebih cepat
/// selama durasi tertentu.
/// </summary>
public class NerfFasterLog : MonoBehaviour
{
    [Header("Faster Log Settings")]
    [Tooltip("Multiplier kecepatan rotasi (misal 1.8 = 80% lebih cepat)")]
    public float speedMultiplier = 1.8f;

    [Tooltip("Durasi efek dalam detik")]
    public float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        // Cari LogController pada parent (log tempat nerf ini menempel)
        LogController log = GetComponentInParent<LogController>();
        if (log != null)
        {
            log.ApplyFasterLog(speedMultiplier, duration);
        }

        Debug.Log($"[Nerf] Faster Log aktif! Speed x{speedMultiplier} selama {duration}s");

        Destroy(gameObject);
    }
}