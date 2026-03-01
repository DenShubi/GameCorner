using UnityEngine;

/// <summary>
/// Power-up Time Slow. Menempel di log, ikut berputar.
/// Saat knife player mengenai power-up ini, rotasi log diperlambat.
/// </summary>
public class PowerUpTimeSlow : MonoBehaviour
{
    [Header("Slow Settings")]
    [Tooltip("Persentase kecepatan saat slow (0.5 = 50% speed)")]
    public float slowMultiplier = 0.5f;

    [Tooltip("Durasi efek slow dalam detik")]
    public float slowDuration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        // Hanya bereaksi terhadap knife yang terbang (tag belum "Knife")
        // Knife yang di-throw punya tag "Untagged" sampai menancap di log
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        // Cari LogController dari parent (power-up ini child dari log)
        LogController log = GetComponentInParent<LogController>();
        if (log != null)
        {
            log.ApplyTimeSlow(slowMultiplier, slowDuration);
        }

        Debug.Log($"[PowerUp] Time Slow aktif! Speed x{slowMultiplier} selama {slowDuration}s");

        // Hancurkan power-up
        Destroy(gameObject);
    }
}