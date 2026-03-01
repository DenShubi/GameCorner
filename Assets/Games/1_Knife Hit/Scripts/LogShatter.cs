using UnityEngine;

/// <summary>
/// Memecah log menggunakan pre-fractured pieces (prefab) dari Blender Cell Fracture.
/// Keping hanya terpental di axis X dan Y (cocok untuk game 2D dengan objek 3D).
/// </summary>
public class LogShatter : MonoBehaviour
{
    [Header("Fracture Pieces")]
    [Tooltip("Array berisi prefab keping-keping log dari Blender.")]
    public GameObject[] piecePrefabs;

    [Header("Explosion Settings")]
    [Tooltip("Kekuatan ledakan (hanya X dan Y)")]
    public float explosionForce = 8f;

    [Tooltip("Kekuatan ke atas tambahan")]
    public float upwardsForce = 2f;

    [Tooltip("Kekuatan putaran acak")]
    public float torqueForce = 1.5f;

    [Tooltip("Berapa detik keping bertahan sebelum dihapus")]
    public float pieceLifetime = 2f;

    public void Shatter()
    {
        if (piecePrefabs == null || piecePrefabs.Length == 0)
        {
            Debug.LogError("[LogShatter] piecePrefabs KOSONG!");
            return;
        }

        // Sembunyikan visual log asli
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;
        foreach (MeshRenderer childMR in GetComponentsInChildren<MeshRenderer>())
        {
            childMR.enabled = false;
        }

        Vector3 logPosition = transform.position;

        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (piecePrefabs[i] == null) continue;

            // Spawn keping
            GameObject piece = Instantiate(
                piecePrefabs[i],
                transform.position,
                transform.rotation
            );
            piece.transform.localScale = transform.localScale;

            // Rigidbody
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            if (rb == null) rb = piece.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = false;

            // ===== KUNCI: Kunci sumbu Z agar hanya gerak di X dan Y =====
            rb.constraints = RigidbodyConstraints.FreezePositionZ;

            // Collider
            if (piece.GetComponent<Collider>() == null)
            {
                MeshCollider mc = piece.AddComponent<MeshCollider>();
                mc.convex = true;
            }

            // ===== GAYA MANUAL HANYA DI X DAN Y =====
            // Hitung arah dari pusat log ke posisi keping (hanya X dan Y)
            Vector3 piecePos = piece.transform.position;
            Vector2 direction2D = new Vector2(
                piecePos.x - logPosition.x,
                piecePos.y - logPosition.y
            );

            // Jika keping tepat di tengah, beri arah acak
            if (direction2D.magnitude < 0.01f)
            {
                direction2D = Random.insideUnitCircle.normalized;
            }
            else
            {
                direction2D = direction2D.normalized;
            }

            // Tambah sedikit random agar tidak terlalu simetris
            direction2D += Random.insideUnitCircle * 0.3f;
            direction2D = direction2D.normalized;

            // Beri gaya HANYA di X dan Y
            Vector3 force = new Vector3(
                direction2D.x * explosionForce,
                direction2D.y * explosionForce + upwardsForce,
                0f  // ← Z selalu 0!
            );

            rb.AddForce(force, ForceMode.Impulse);

            // Putaran HANYA di sumbu Z (seperti 2D spin)
            rb.AddTorque(new Vector3(0f, 0f, Random.Range(-torqueForce, torqueForce)), ForceMode.Impulse);

            // Auto-destroy
            Destroy(piece, pieceLifetime);
        }

        Debug.Log($"[LogShatter] Log terpecah menjadi {piecePrefabs.Length} keping!");
    }
}