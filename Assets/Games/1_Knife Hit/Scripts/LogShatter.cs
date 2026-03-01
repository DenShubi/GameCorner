using UnityEngine;

/// <summary>
/// Memecah log menggunakan pre-fractured pieces (prefab) dari Blender Cell Fracture.
/// Saat log hancur, spawn keping-keping prefab dan beri explosion force.
/// </summary>
public class LogShatter : MonoBehaviour
{
    [Header("Fracture Pieces")]
    [Tooltip("Array berisi prefab keping-keping log dari Blender. Drag semua keping prefab ke sini.")]
    public GameObject[] piecePrefabs;

    [Header("Explosion Settings")]
    [Tooltip("Kekuatan ledakan yang mendorong keping")]
    public float explosionForce = 300f;

    [Tooltip("Radius ledakan")]
    public float explosionRadius = 3f;

    [Tooltip("Modifier ke atas agar keping menyebar ke atas juga")]
    public float upwardsModifier = 0.5f;

    [Tooltip("Kekuatan putaran acak pada keping")]
    public float torqueForce = 10f;

    [Tooltip("Berapa detik keping bertahan sebelum dihapus")]
    public float pieceLifetime = 3f;

    /// <summary>
    /// Panggil method ini untuk memecah log.
    /// Spawn semua keping prefab dan beri gaya ledakan.
    /// </summary>
    public void Shatter()
    {
        if (piecePrefabs == null || piecePrefabs.Length == 0)
        {
            Debug.LogError("[LogShatter] piecePrefabs KOSONG! " +
                           "Drag keping prefab ke array Piece Prefabs di Inspector.");
            return;
        }

        // Sembunyikan visual log asli
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        foreach (MeshRenderer childMR in GetComponentsInChildren<MeshRenderer>())
        {
            childMR.enabled = false;
        }

        Vector3 explosionCenter = transform.position;

        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (piecePrefabs[i] == null) continue;

            // Spawn keping di posisi dan rotasi log saat ini
            GameObject piece = Instantiate(
                piecePrefabs[i],
                transform.position,
                transform.rotation
            );

            piece.transform.localScale = transform.localScale;

            // Tambahkan Rigidbody
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null) rb = piece.AddComponent<Rigidbody>();
            rb.mass = 0.3f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // Tambahkan Collider jika belum ada
            if (piece.GetComponent<Collider>() == null)
            {
                MeshCollider mc = piece.AddComponent<MeshCollider>();
                mc.convex = true;
            }

            // Gaya ledakan
            rb.AddExplosionForce(explosionForce, explosionCenter, explosionRadius, upwardsModifier);
            rb.AddTorque(Random.insideUnitSphere * torqueForce, ForceMode.Impulse);

            // Auto-destroy
            Destroy(piece, pieceLifetime);
        }

        Debug.Log($"[LogShatter] Log terpecah menjadi {piecePrefabs.Length} keping!");
    }
}