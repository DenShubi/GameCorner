using UnityEngine;

/// <summary>
/// Power-up Double Hit. Menempel di log, ikut berputar.
/// Saat knife player mengenai power-up ini, damage knife menjadi 2x
/// untuk beberapa hit berikutnya.
/// </summary>
public class PowerUpDoubleHit : MonoBehaviour
{
    [Header("Double Hit Settings")]
    [Tooltip("Berapa hit yang mendapat double damage")]
    public int doubleHitCount = 3;

    private void OnTriggerEnter(Collider other)
    {
        // Hanya bereaksi terhadap knife yang punya KnifeController
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        // Aktifkan double hit di GameManager
        GameManager.instance.ActivateDoubleHit(doubleHitCount);

        Debug.Log($"[PowerUp] Double Hit aktif! {doubleHitCount} hit berikutnya damage x2");

        // Hancurkan power-up
        Destroy(gameObject);
    }
}