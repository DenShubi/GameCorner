using UnityEngine;

/// <summary>
/// Power-up Score Multiplier. Menempel di log, ikut berputar.
/// Saat knife player mengenai power-up ini, score dikalikan
/// selama durasi tertentu.
/// </summary>
public class PowerUpScoreMultiplier : MonoBehaviour
{
    [Header("Multiplier Settings")]
    [Tooltip("Pengali score (3 = score x3)")]
    public int multiplier = 3;

    [Tooltip("Durasi efek multiplier dalam detik")]
    public float duration = 10f;

    private void OnTriggerEnter(Collider other)
    {
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        GameManager.instance.ActivateScoreMultiplier(multiplier, duration);

        Debug.Log($"[PowerUp] Score Multiplier x{multiplier} aktif selama {duration}s!");

        Destroy(gameObject);
    }
}