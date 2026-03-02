using UnityEngine;

/// <summary>
/// Nerf Fog Screen. Menempel di log, ikut berputar.
/// Saat knife player mengenai nerf ini, layar tertutup kabut semi-transparan
/// selama durasi tertentu.
/// </summary>
public class NerfFogScreen : MonoBehaviour
{
    [Header("Fog Settings")]
    [Tooltip("Durasi kabut dalam detik")]
    public float duration = 5f;

    [Tooltip("Opacity kabut (0 = transparan penuh, 1 = solid penuh)")]
    [Range(0f, 1f)]
    public float fogOpacity = 0.7f;

    private void OnTriggerEnter(Collider other)
    {
        KnifeController knife = other.GetComponent<KnifeController>();
        if (knife == null) return;

        // Cari FogScreenUI di scene dan aktifkan
        FogScreenUI fogUI = FindObjectOfType<FogScreenUI>(true);
        if (fogUI != null)
        {
            fogUI.ActivateFog(duration, fogOpacity);
        }

        Debug.Log($"[Nerf] Fog Screen aktif! Opacity {fogOpacity} selama {duration}s");

        Destroy(gameObject);
    }
}