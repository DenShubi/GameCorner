using UnityEngine;

/// <summary>
/// Power-up Shield. Menempel di log, ikut berputar.
/// Saat knife player mengenai power-up ini, player mendapat shield
/// yang melindungi dari 1x collision dengan obstacle.
/// </summary>
public class PowerUpShield : MonoBehaviour
{
    [Header("Shield Settings")]
    [Tooltip("Jumlah hit yang bisa diserap oleh shield")]
    public int shieldCharges = 1;

    private void OnTriggerEnter(Collider other)
    {
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        GameManager.instance.ActivateShield(shieldCharges);

        Debug.Log($"[PowerUp] Shield aktif! {shieldCharges} hit terlindungi");

        Destroy(gameObject);
    }
}